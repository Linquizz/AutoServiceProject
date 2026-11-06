using System.Text.Json;

namespace AutoService.Services;

public class AppSettings
{
    public static AppSettings Current { get; private set; } = new();

    public static string? LoadError { get; private set; }

    public string ConnectionString { get; set; } =
        "Server=.\\SQLEXPRESS;Database=AutoServiceDB;Trusted_Connection=True;TrustServerCertificate=True;";

    public bool SeedDemoData { get; set; } = true;

    public PathSettings Paths { get; set; } = new();
    public SmtpSettings Smtp { get; set; } = new();

    public static void Load()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            var main = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(main))
            {
                LoadError = "Файл appsettings.json не найден рядом с программой. Используются значения по умолчанию.";
                Current = new AppSettings();
                return;
            }

            Current = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(main), options) ?? new AppSettings();

            var local = Path.Combine(AppContext.BaseDirectory, "appsettings.Local.json");
            if (File.Exists(local))
            {
                var overrides = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(local), options);
                if (overrides != null)
                {
                    if (!string.IsNullOrWhiteSpace(overrides.Smtp.Host) || !string.IsNullOrWhiteSpace(overrides.Smtp.UserName))
                        Current.Smtp = overrides.Smtp;
                    if (!string.IsNullOrWhiteSpace(overrides.ConnectionString)
                        && overrides.ConnectionString != new AppSettings().ConnectionString)
                        Current.ConnectionString = overrides.ConnectionString;
                }
            }
        }
        catch (Exception ex)
        {
            LoadError = "Ошибка чтения appsettings.json: " + ex.Message;
            Current = new AppSettings();
        }
    }
}

public class PathSettings
{
    public string WorkOrdersFolder { get; set; } = "";
}

public class SmtpSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 465;

    public string SecureMode { get; set; } = "SslOnConnect";

    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromAddress { get; set; } = "";
    public string FromName { get; set; } = "Автосервис";
}
