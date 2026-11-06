namespace AutoService.Services;

public static class AppPaths
{
    public static string BaseDirectory => AppContext.BaseDirectory;

    public static string Resolve(string path) =>
        Path.IsPathRooted(path) ? path : Path.Combine(BaseDirectory, path);

    public static string WorkOrdersFolder
    {
        get
        {
            var configured = AppSettings.Current.Paths.WorkOrdersFolder;
            if (string.IsNullOrWhiteSpace(configured))
            {
                configured = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "AutoService", "ЗаказНаряды");
            }
            return Resolve(configured);
        }
    }
}
