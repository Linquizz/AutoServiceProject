using System.Diagnostics;

namespace AutoService.Services;

public static class FileLauncher
{
    public static void Open(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Не удалось открыть «{path}»: {ex.Message}");
        }
    }

    public static void OpenFolder(string folder)
    {
        Directory.CreateDirectory(folder);
        Open(folder);
    }
}
