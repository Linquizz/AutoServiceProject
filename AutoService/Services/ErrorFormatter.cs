using Microsoft.Data.SqlClient;

namespace AutoService.Services;

public static class ErrorFormatter
{
    public static string Format(Exception ex)
    {
        if (ex is BusinessException) return ex.Message;

        var sql = FindSqlException(ex);
        if (sql != null)
        {
            return sql.Number switch
            {
                547 => "Операция невозможна: запись используется в других данных (например, у марки есть модели, а у клиента или автомобиля - связанные записи).",
                2601 or 2627 => "Такая запись уже существует (нарушено условие уникальности).",
                2628 or 8152 => "Одно из значений слишком длинное для сохранения в базе данных.",
                3201 or 3013 or 3041 => "SQL Server не смог записать файл резервной копии. Выберите папку, доступную службе SQL Server, " +
                                        "или оставьте поле пустым - тогда будет использована папка резервных копий сервера по умолчанию.",
                -1 or 2 or 53 or 4060 or 18456 or 40 =>
                    "Не удалось подключиться к базе данных. Проверьте, что SQL Server запущен, и строку подключения " +
                    "ConnectionString в файле appsettings.json.\n\nПодробности: " + sql.Message,
                _ => $"Ошибка базы данных (код {sql.Number}): {sql.Message}"
            };
        }

        return "Непредвиденная ошибка: " + ex.GetBaseException().Message;
    }

    private static SqlException? FindSqlException(Exception? ex)
    {
        while (ex != null)
        {
            if (ex is SqlException s) return s;
            ex = ex.InnerException;
        }
        return null;
    }
}
