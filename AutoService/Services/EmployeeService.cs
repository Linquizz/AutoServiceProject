using AutoService.Data;
using AutoService.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Services;

public class EmployeeService
{
    public async Task<List<Employee>> GetAllAsync()
    {
        await using var db = new AutoServiceDbContext();
        return await db.Employees.AsNoTracking().OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToListAsync();
    }

    public async Task AddAsync(Employee employee, string? password)
    {
        EnsureAdmin();
        Normalize(employee);
        Validator.ValidateEmployee(employee);

        await using var db = new AutoServiceDbContext();
        if (employee.Login != null)
        {
            if (string.IsNullOrEmpty(password))
                throw new BusinessException("Для сотрудника с логином необходимо задать пароль.");
            CheckPassword(password);
            if (await db.Employees.AnyAsync(e => e.Login == employee.Login))
                throw new BusinessException($"Логин «{employee.Login}» уже занят.");
            employee.PasswordHash = PasswordHasher.Hash(password);
        }
        else
        {
            employee.PasswordHash = null;
        }

        employee.Id = 0;
        db.Employees.Add(employee);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Employee employee, string? password)
    {
        EnsureAdmin();
        Normalize(employee);
        Validator.ValidateEmployee(employee);

        await using var db = new AutoServiceDbContext();
        var existing = await db.Employees.FindAsync(employee.Id) ?? throw new BusinessException("Сотрудник не найден.");

        if (existing.Id == Session.User?.Id && employee.Role != Roles.Admin)
            throw new BusinessException("Нельзя снять права администратора у самого себя.");

        if (employee.Login != null)
        {
            if (await db.Employees.AnyAsync(e => e.Login == employee.Login && e.Id != employee.Id))
                throw new BusinessException($"Логин «{employee.Login}» уже занят.");

            if (!string.IsNullOrEmpty(password))
            {
                CheckPassword(password);
                existing.PasswordHash = PasswordHasher.Hash(password);
            }
            else if (string.IsNullOrEmpty(existing.PasswordHash))
            {
                throw new BusinessException("Для сотрудника с логином необходимо задать пароль.");
            }
        }
        else
        {
            if (existing.Id == Session.User?.Id)
                throw new BusinessException("Нельзя удалить собственный логин.");
            existing.PasswordHash = null;
        }

        existing.LastName = employee.LastName;
        existing.FirstName = employee.FirstName;
        existing.MiddleName = employee.MiddleName;
        existing.Phone = employee.Phone;
        existing.Position = employee.Position;
        existing.Role = employee.Role;
        existing.Login = employee.Login;
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int employeeId)
    {
        EnsureAdmin();
        if (employeeId == Session.User?.Id)
            throw new BusinessException("Нельзя удалить самого себя.");

        await using var db = new AutoServiceDbContext();
        var existing = await db.Employees.FindAsync(employeeId);
        if (existing == null) return;

        if (await db.OrderServices.AnyAsync(o => o.EmployeeId == employeeId))
            throw new BusinessException("Нельзя удалить сотрудника: он указан исполнителем в заказ-нарядах.");

        db.Employees.Remove(existing);
        await db.SaveChangesAsync();
    }

    private static void EnsureAdmin()
    {
        if (!Session.IsAdmin)
            throw new BusinessException("Управлять сотрудниками может только администратор.");
    }

    private static void CheckPassword(string password)
    {
        if (password.Length < 6)
            throw new BusinessException("Пароль должен содержать не менее 6 символов.");
    }

    private static void Normalize(Employee e)
    {
        e.LastName = (e.LastName ?? "").Trim();
        e.FirstName = (e.FirstName ?? "").Trim();
        e.MiddleName = string.IsNullOrWhiteSpace(e.MiddleName) ? null : e.MiddleName.Trim();
        e.Phone = (e.Phone ?? "").Trim();
        e.Position = (e.Position ?? "").Trim();
        e.Login = string.IsNullOrWhiteSpace(e.Login) ? null : e.Login.Trim();
    }
}
