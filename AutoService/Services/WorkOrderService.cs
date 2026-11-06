using AutoService.Data;
using AutoService.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Services;

public class WorkOrderService
{
    private static IQueryable<WorkOrder> FullQuery(AutoServiceDbContext db) =>
        db.WorkOrders.AsNoTracking().AsSplitQuery()
            .Include(w => w.Car).ThenInclude(c => c!.Client)
            .Include(w => w.Car).ThenInclude(c => c!.Model).ThenInclude(m => m!.Brand)
            .Include(w => w.Works).ThenInclude(x => x.Service)
            .Include(w => w.Works).ThenInclude(x => x.Employee)
            .Include(w => w.Parts).ThenInclude(p => p.Part);

    public async Task<List<WorkOrder>> SearchAsync(string? text, bool? completed, DateTime? from, DateTime? to)
    {
        if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
            throw new BusinessException("Начальная дата не может быть позже конечной.");

        await using var db = new AutoServiceDbContext();
        var query = FullQuery(db);

        if (completed.HasValue)
            query = query.Where(w => w.IsCompleted == completed.Value);
        if (from.HasValue)
        {
            var start = from.Value.Date;
            query = query.Where(w => w.StartDate >= start);
        }
        if (to.HasValue)
        {
            var end = to.Value.Date.AddDays(1);
            query = query.Where(w => w.StartDate < end);
        }
        if (!string.IsNullOrWhiteSpace(text))
        {
            var t = text.Trim();
            var plate = Validator.NormalizePlate(t);
            int.TryParse(t, out var id);
            query = query.Where(w => w.Id == id
                                     || w.RegistrationNumber.Contains(plate)
                                     || (w.Car!.VIN != null && w.Car.VIN.Contains(t))
                                     || w.Car!.Client!.LastName.Contains(t)
                                     || w.Car.Client.FirstName.Contains(t)
                                     || w.Car.Client.Phone.Contains(t)
                                     || w.Car.Model!.Name.Contains(t)
                                     || w.Car.Model.Brand!.Name.Contains(t));
        }

        return await query.OrderByDescending(w => w.StartDate).ThenByDescending(w => w.Id).ToListAsync();
    }

    public async Task<WorkOrder?> GetAsync(int id)
    {
        await using var db = new AutoServiceDbContext();
        return await FullQuery(db).FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<List<WorkOrder>> GetByCarAsync(string registrationNumber)
    {
        await using var db = new AutoServiceDbContext();
        return await FullQuery(db)
            .Where(w => w.RegistrationNumber == registrationNumber)
            .OrderByDescending(w => w.StartDate).ThenByDescending(w => w.Id)
            .ToListAsync();
    }

    public async Task<int> CreateAsync(WorkOrder order)
    {
        ValidateHeader(order);

        await using var db = new AutoServiceDbContext();
        if (!await db.Cars.AnyAsync(c => c.RegistrationNumber == order.RegistrationNumber))
            throw new BusinessException("Автомобиль не найден.");

        var entity = new WorkOrder
        {
            RegistrationNumber = order.RegistrationNumber,
            StartDate = DateTime.Now,
            IsCompleted = false,
            Reason = Clean(order.Reason)
        };

        db.WorkOrders.Add(entity);
        await db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateHeaderAsync(WorkOrder order)
    {
        ValidateHeader(order);

        await using var db = new AutoServiceDbContext();
        var entity = await db.WorkOrders.FindAsync(order.Id) ?? throw new BusinessException("Заказ-наряд не найден.");
        EnsureOpen(entity);

        entity.Reason = Clean(order.Reason);
        await db.SaveChangesAsync();
    }

    public async Task SetCompletedAsync(int orderId, bool completed)
    {
        await using var db = new AutoServiceDbContext();
        var entity = await db.WorkOrders.Include(w => w.Works).FirstOrDefaultAsync(w => w.Id == orderId)
                     ?? throw new BusinessException("Заказ-наряд не найден.");

        if (completed)
        {
            if (entity.IsCompleted) return;
            if (entity.Works.Count == 0)
                throw new BusinessException("Нельзя завершить заказ-наряд без выполненных работ. Добавьте хотя бы одну работу.");
            entity.IsCompleted = true;
            entity.EndDate = DateTime.Now;
        }
        else
        {
            entity.IsCompleted = false;
            entity.EndDate = null;
        }

        await db.SaveChangesAsync();
    }

    public async Task AddWorkAsync(int orderId, OrderService work)
    {
        ValidateWork(work);

        await using var db = new AutoServiceDbContext();
        var order = await db.WorkOrders.FindAsync(orderId) ?? throw new BusinessException("Заказ-наряд не найден.");
        EnsureOpen(order);

        db.OrderServices.Add(new OrderService
        {
            WorkOrderId = orderId,
            ServiceId = work.ServiceId,
            EmployeeId = work.EmployeeId,
            Price = work.Price,
            Duration = work.Duration
        });
        await db.SaveChangesAsync();
    }

    public async Task UpdateWorkAsync(OrderService work)
    {
        ValidateWork(work);

        await using var db = new AutoServiceDbContext();
        var entity = await db.OrderServices.Include(o => o.WorkOrder).FirstOrDefaultAsync(o => o.Id == work.Id)
                     ?? throw new BusinessException("Работа не найдена.");
        EnsureOpen(entity.WorkOrder!);

        entity.EmployeeId = work.EmployeeId;
        entity.Duration = work.Duration;
        await db.SaveChangesAsync();
    }

    public async Task DeleteWorkAsync(int workId)
    {
        await using var db = new AutoServiceDbContext();
        var entity = await db.OrderServices.Include(o => o.WorkOrder).FirstOrDefaultAsync(o => o.Id == workId);
        if (entity == null) return;
        EnsureOpen(entity.WorkOrder!);

        db.OrderServices.Remove(entity);
        await db.SaveChangesAsync();
    }

    public async Task AddPartAsync(int orderId, string article, int quantity)
    {
        if (quantity <= 0) throw new BusinessException("Количество должно быть положительным целым числом.");

        await using var db = new AutoServiceDbContext();
        var order = await db.WorkOrders.FindAsync(orderId) ?? throw new BusinessException("Заказ-наряд не найден.");
        EnsureOpen(order);

        var part = await db.Parts.FindAsync(article) ?? throw new BusinessException("Запасная часть не найдена в справочнике.");
        if (quantity > part.StockQuantity)
            throw new BusinessException($"Недостаточно на складе: доступно {part.StockQuantity} шт.");

        part.StockQuantity -= quantity;

        var existing = await db.OrderParts.FirstOrDefaultAsync(p => p.WorkOrderId == orderId && p.Article == article);
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            db.OrderParts.Add(new OrderPart
            {
                WorkOrderId = orderId,
                Article = article,
                Quantity = quantity,
                Price = part.Price
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task UpdatePartAsync(int orderPartId, int newQuantity)
    {
        if (newQuantity <= 0) throw new BusinessException("Количество должно быть положительным целым числом.");

        await using var db = new AutoServiceDbContext();
        var entity = await db.OrderParts.Include(p => p.WorkOrder).Include(p => p.Part)
                         .FirstOrDefaultAsync(p => p.Id == orderPartId)
                     ?? throw new BusinessException("Позиция не найдена.");
        EnsureOpen(entity.WorkOrder!);

        var delta = newQuantity - entity.Quantity;
        if (delta > entity.Part!.StockQuantity)
            throw new BusinessException($"Недостаточно на складе: можно добавить ещё не более {entity.Part.StockQuantity} шт.");

        entity.Part.StockQuantity -= delta;
        entity.Quantity = newQuantity;
        await db.SaveChangesAsync();
    }

    public async Task DeletePartAsync(int orderPartId)
    {
        await using var db = new AutoServiceDbContext();
        var entity = await db.OrderParts.Include(p => p.WorkOrder).Include(p => p.Part)
            .FirstOrDefaultAsync(p => p.Id == orderPartId);
        if (entity == null) return;
        EnsureOpen(entity.WorkOrder!);

        if (entity.Part != null) entity.Part.StockQuantity += entity.Quantity;
        db.OrderParts.Remove(entity);
        await db.SaveChangesAsync();
    }

    private static void EnsureOpen(WorkOrder order)
    {
        if (order.IsCompleted)
            throw new BusinessException("Заказ-наряд завершён и доступен только для просмотра. Верните его в работу, чтобы внести изменения.");
    }

    private static void ValidateHeader(WorkOrder order)
    {
        if (string.IsNullOrWhiteSpace(order.RegistrationNumber))
            throw new BusinessException("Выберите автомобиль клиента.");
        if (!string.IsNullOrWhiteSpace(order.Reason) && order.Reason.Trim().Length > 500)
            throw new BusinessException("Причина обращения не может быть длиннее 500 символов.");
    }

    private static void ValidateWork(OrderService work)
    {
        if (work.ServiceId <= 0) throw new BusinessException("Выберите услугу.");
        if (work.EmployeeId <= 0) throw new BusinessException("Выберите исполнителя.");
        if (work.Price < 0) throw new BusinessException("Стоимость работы не может быть отрицательной.");
        if (work.Duration is < 0) throw new BusinessException("Продолжительность не может быть отрицательной.");
    }

    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
