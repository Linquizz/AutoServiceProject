using AutoService.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Services;

public class BackupService
{
    public async Task<string> CreateBackupAsync(string? folder)
    {
        if (!Session.IsAdmin)
            throw new BusinessException("Создавать резервные копии может только администратор.");

        await using var db = new AutoServiceDbContext();
        db.Database.SetCommandTimeout(600);
        var databaseName = db.Database.GetDbConnection().Database;

        if (string.IsNullOrWhiteSpace(folder))
        {
            var defaults = await db.Database
                .SqlQueryRaw<string>("SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(512)) AS [Value]")
                .ToListAsync();
            folder = defaults.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(folder))
                throw new BusinessException("Не удалось определить папку резервных копий сервера. Укажите папку вручную.");
        }

        var file = Path.Combine(folder, $"{databaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
        var sql = "BACKUP DATABASE [" + databaseName.Replace("]", "]]") + "] TO DISK = {0} WITH INIT";
        await db.Database.ExecuteSqlRawAsync(sql, file);
        return file;
    }
}
