using AutoService.Data;
using AutoService.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Services;

public class AuthService
{
    public async Task<Employee?> LoginAsync(string login, string password)
    {
        login = login.Trim();
        await using var db = new AutoServiceDbContext();
        var employee = await db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Login == login);
        if (employee == null) return null;
        return PasswordHasher.Verify(password, employee.PasswordHash) ? employee : null;
    }
}
