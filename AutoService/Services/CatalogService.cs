using AutoService.Data;
using AutoService.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Services;

public class CatalogService
{
    private static void EnsureAdmin()
    {
        if (!Session.IsAdmin)
            throw new BusinessException("Редактировать справочники может только администратор.");
    }

    public async Task<List<Brand>> GetBrandsAsync()
    {
        await using var db = new AutoServiceDbContext();
        var brands = await db.Brands.AsNoTracking().Include(b => b.Models).OrderBy(b => b.Name).ToListAsync();
        foreach (var b in brands) b.Models = b.Models.OrderBy(m => m.Name).ToList();
        return brands;
    }

    public async Task<int> AddBrandAsync(string name)
    {
        EnsureAdmin();
        name = RequireName(name, "Название марки", 50);

        await using var db = new AutoServiceDbContext();
        if (await db.Brands.AnyAsync(b => b.Name == name))
            throw new BusinessException($"Марка «{name}» уже есть в справочнике.");

        var brand = new Brand { Name = name };
        db.Brands.Add(brand);
        await db.SaveChangesAsync();
        return brand.Id;
    }

    public async Task RenameBrandAsync(int brandId, string name)
    {
        EnsureAdmin();
        name = RequireName(name, "Название марки", 50);

        await using var db = new AutoServiceDbContext();
        if (await db.Brands.AnyAsync(b => b.Name == name && b.Id != brandId))
            throw new BusinessException($"Марка «{name}» уже есть в справочнике.");

        var brand = await db.Brands.FindAsync(brandId) ?? throw new BusinessException("Марка не найдена.");
        brand.Name = name;
        await db.SaveChangesAsync();
    }

    public async Task DeleteBrandAsync(int brandId)
    {
        EnsureAdmin();
        await using var db = new AutoServiceDbContext();
        var brand = await db.Brands.Include(b => b.Models).FirstOrDefaultAsync(b => b.Id == brandId);
        if (brand == null) return;
        if (brand.Models.Count > 0)
            throw new BusinessException("Нельзя удалить марку, у которой есть модели. Сначала удалите её модели.");

        db.Brands.Remove(brand);
        await db.SaveChangesAsync();
    }

    public async Task AddModelAsync(int brandId, string name)
    {
        EnsureAdmin();
        name = RequireName(name, "Название модели", 50);

        await using var db = new AutoServiceDbContext();
        if (await db.CarModels.AnyAsync(m => m.BrandId == brandId && m.Name == name))
            throw new BusinessException($"Модель «{name}» у этой марки уже есть.");

        db.CarModels.Add(new CarModel { BrandId = brandId, Name = name });
        await db.SaveChangesAsync();
    }

    public async Task RenameModelAsync(int modelId, string name)
    {
        EnsureAdmin();
        name = RequireName(name, "Название модели", 50);

        await using var db = new AutoServiceDbContext();
        var model = await db.CarModels.FindAsync(modelId) ?? throw new BusinessException("Модель не найдена.");
        if (await db.CarModels.AnyAsync(m => m.BrandId == model.BrandId && m.Name == name && m.Id != modelId))
            throw new BusinessException($"Модель «{name}» у этой марки уже есть.");

        model.Name = name;
        await db.SaveChangesAsync();
    }

    public async Task DeleteModelAsync(int modelId)
    {
        EnsureAdmin();
        await using var db = new AutoServiceDbContext();
        var model = await db.CarModels.FindAsync(modelId);
        if (model == null) return;
        if (await db.Cars.AnyAsync(c => c.ModelId == modelId))
            throw new BusinessException("Нельзя удалить модель: есть зарегистрированные автомобили этой модели.");

        db.CarModels.Remove(model);
        await db.SaveChangesAsync();
    }

    public async Task<List<Service>> GetServicesAsync(string? search = null)
    {
        await using var db = new AutoServiceDbContext();
        IQueryable<Service> query = db.Services.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var t = search.Trim();
            query = query.Where(s => s.Name.Contains(t) || (s.Description != null && s.Description.Contains(t)));
        }
        return await query.OrderBy(s => s.Name).ToListAsync();
    }

    public async Task SaveServiceAsync(Service service)
    {
        EnsureAdmin();
        service.Name = (service.Name ?? "").Trim();
        service.Description = string.IsNullOrWhiteSpace(service.Description) ? null : service.Description.Trim();
        Validator.ValidateService(service);

        await using var db = new AutoServiceDbContext();
        if (service.Id == 0)
        {
            db.Services.Add(service);
        }
        else
        {
            var existing = await db.Services.FindAsync(service.Id) ?? throw new BusinessException("Услуга не найдена.");
            existing.Name = service.Name;
            existing.Description = service.Description;
            existing.Price = service.Price;
        }
        await db.SaveChangesAsync();
    }

    public async Task DeleteServiceAsync(int serviceId)
    {
        EnsureAdmin();
        await using var db = new AutoServiceDbContext();
        var service = await db.Services.FindAsync(serviceId);
        if (service == null) return;
        if (await db.OrderServices.AnyAsync(o => o.ServiceId == serviceId))
            throw new BusinessException("Нельзя удалить услугу: она использована в заказ-нарядах.");

        db.Services.Remove(service);
        await db.SaveChangesAsync();
    }

    public async Task<List<Part>> GetPartsAsync(string? search = null, bool onlyInStock = false, string? alwaysIncludeArticle = null)
    {
        await using var db = new AutoServiceDbContext();
        IQueryable<Part> query = db.Parts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var t = search.Trim();
            query = query.Where(p => p.Name.Contains(t) || p.Article.Contains(t));
        }
        if (onlyInStock)
            query = query.Where(p => p.StockQuantity > 0 || p.Article == alwaysIncludeArticle);

        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task AddPartAsync(Part part)
    {
        EnsureAdmin();
        Normalize(part);
        Validator.ValidatePart(part);

        await using var db = new AutoServiceDbContext();
        if (await db.Parts.AnyAsync(p => p.Article == part.Article))
            throw new BusinessException($"Запасная часть с артикулом «{part.Article}» уже есть в справочнике.");

        db.Parts.Add(part);
        await db.SaveChangesAsync();
    }

    public async Task UpdatePartAsync(Part part)
    {
        EnsureAdmin();
        Normalize(part);
        Validator.ValidatePart(part);

        await using var db = new AutoServiceDbContext();
        var existing = await db.Parts.FindAsync(part.Article) ?? throw new BusinessException("Запасная часть не найдена.");
        existing.Name = part.Name;
        existing.Price = part.Price;
        existing.StockQuantity = part.StockQuantity;
        await db.SaveChangesAsync();
    }

    public async Task DeletePartAsync(string article)
    {
        EnsureAdmin();
        await using var db = new AutoServiceDbContext();
        var part = await db.Parts.FindAsync(article);
        if (part == null) return;
        if (await db.OrderParts.AnyAsync(o => o.Article == article))
            throw new BusinessException("Нельзя удалить запасную часть: она использована в заказ-нарядах.");

        db.Parts.Remove(part);
        await db.SaveChangesAsync();
    }

    private static void Normalize(Part p)
    {
        p.Article = (p.Article ?? "").Trim();
        p.Name = (p.Name ?? "").Trim();
    }

    private static string RequireName(string? name, string field, int maxLength)
    {
        name = (name ?? "").Trim();
        if (name.Length == 0) throw new BusinessException($"Поле «{field}» обязательно для заполнения.");
        if (name.Length > maxLength) throw new BusinessException($"Поле «{field}» не может быть длиннее {maxLength} символов.");
        return name;
    }
}
