using AutoService.Data;
using AutoService.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Services;

public class CarService
{
    public static readonly string[] EngineTypes = { "Бензин", "Дизель", "Газ/бензин", "Гибрид", "Электро" };

    public async Task<List<Car>> GetAllAsync(string? search = null)
    {
        await using var db = new AutoServiceDbContext();
        IQueryable<Car> query = db.Cars.AsNoTracking()
            .Include(c => c.Client)
            .Include(c => c.Model).ThenInclude(m => m!.Brand);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var t = search.Trim();
            var plate = Validator.NormalizePlate(t);
            query = query.Where(c => c.RegistrationNumber.Contains(plate)
                                     || (c.VIN != null && c.VIN.Contains(t))
                                     || c.Client!.LastName.Contains(t)
                                     || c.Client.FirstName.Contains(t)
                                     || c.Model!.Name.Contains(t)
                                     || c.Model.Brand!.Name.Contains(t));
        }

        return await query.OrderBy(c => c.RegistrationNumber).ToListAsync();
    }

    public async Task AddAsync(Car car)
    {
        Normalize(car);
        Validator.ValidateCar(car);

        await using var db = new AutoServiceDbContext();
        if (await db.Cars.AnyAsync(c => c.RegistrationNumber == car.RegistrationNumber))
            throw new BusinessException($"Автомобиль с номером {car.RegistrationNumber} уже зарегистрирован.");

        car.Client = null;
        car.Model = null;
        car.WorkOrders = new List<WorkOrder>();
        db.Cars.Add(car);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Car car)
    {
        Normalize(car);
        Validator.ValidateCar(car);

        await using var db = new AutoServiceDbContext();
        var existing = await db.Cars.FindAsync(car.RegistrationNumber)
                       ?? throw new BusinessException("Автомобиль не найден.");

        existing.ClientId = car.ClientId;
        existing.ModelId = car.ModelId;
        existing.VIN = car.VIN;
        existing.Year = car.Year;
        existing.EngineType = car.EngineType;
        existing.Mileage = car.Mileage;
        await db.SaveChangesAsync();
    }

    private static void Normalize(Car car)
    {
        car.RegistrationNumber = Validator.NormalizePlate(car.RegistrationNumber);
        car.VIN = string.IsNullOrWhiteSpace(car.VIN) ? null : car.VIN.Trim().ToUpperInvariant();
        car.EngineType = (car.EngineType ?? "").Trim();
    }
}
