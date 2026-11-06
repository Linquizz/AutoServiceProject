using AutoService.Data;
using AutoService.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Services;

public class ClientService
{
    public async Task<List<Client>> GetAllAsync(string? search = null)
    {
        await using var db = new AutoServiceDbContext();
        IQueryable<Client> query = db.Clients.AsNoTracking().Include(c => c.Cars);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var t = search.Trim();
            query = query.Where(c => c.LastName.Contains(t) || c.FirstName.Contains(t)
                                     || (c.MiddleName != null && c.MiddleName.Contains(t))
                                     || c.Phone.Contains(t)
                                     || (c.Email != null && c.Email.Contains(t))
                                     || c.Cars.Any(car => car.RegistrationNumber.Contains(t)));
        }

        return await query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToListAsync();
    }

    public async Task<List<Client>> GetAllWithCarsAsync()
    {
        await using var db = new AutoServiceDbContext();
        return await db.Clients.AsNoTracking()
            .Include(c => c.Cars).ThenInclude(car => car.Model).ThenInclude(m => m!.Brand)
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .ToListAsync();
    }

    public async Task<int> AddAsync(Client client)
    {
        Normalize(client);
        Validator.ValidateClient(client);

        await using var db = new AutoServiceDbContext();
        client.Id = 0;
        client.Cars = new List<Car>();
        db.Clients.Add(client);
        await db.SaveChangesAsync();
        return client.Id;
    }

    public async Task UpdateAsync(Client client)
    {
        Normalize(client);
        Validator.ValidateClient(client);

        await using var db = new AutoServiceDbContext();
        var existing = await db.Clients.FindAsync(client.Id)
                       ?? throw new BusinessException("Клиент не найден (возможно, уже удалён).");

        existing.LastName = client.LastName;
        existing.FirstName = client.FirstName;
        existing.MiddleName = client.MiddleName;
        existing.Phone = client.Phone;
        existing.Email = client.Email;
        await db.SaveChangesAsync();
    }

    public async Task<(int Cars, int Orders)> GetDeleteInfoAsync(int clientId)
    {
        await using var db = new AutoServiceDbContext();
        var cars = await db.Cars.CountAsync(c => c.ClientId == clientId);
        var orders = await db.WorkOrders.CountAsync(w => w.Car!.ClientId == clientId);
        return (cars, orders);
    }

    public async Task DeleteAsync(int clientId)
    {
        if (!Session.IsAdmin)
            throw new BusinessException("Удалять клиентов может только администратор.");

        await using var db = new AutoServiceDbContext();
        var existing = await db.Clients.FindAsync(clientId);
        if (existing == null) return;

        db.Clients.Remove(existing);
        await db.SaveChangesAsync();
    }

    private static void Normalize(Client c)
    {
        c.LastName = (c.LastName ?? "").Trim();
        c.FirstName = (c.FirstName ?? "").Trim();
        c.MiddleName = string.IsNullOrWhiteSpace(c.MiddleName) ? null : c.MiddleName.Trim();
        c.Phone = (c.Phone ?? "").Trim();
        c.Email = string.IsNullOrWhiteSpace(c.Email) ? null : c.Email.Trim();
    }
}
