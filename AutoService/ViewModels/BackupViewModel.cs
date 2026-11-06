using AutoService.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoService.ViewModels;

public partial class BackupViewModel : ViewModelBase
{
    private readonly BackupService _service = new();

    [ObservableProperty] private string _folderPath = "";
    [ObservableProperty] private string _lastBackupText = "Резервная копия в этом сеансе ещё не создавалась.";
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    private async Task BrowseAsync()
    {
        var folder = await DialogService.PickFolderAsync("Папка для резервной копии");
        if (folder != null) FolderPath = folder;
    }

    [RelayCommand]
    private async Task CreateBackupAsync() => await RunAsync(async () =>
    {
        IsBusy = true;
        try
        {
            var file = await _service.CreateBackupAsync(FolderPath);
            LastBackupText = $"Последняя копия: {DateTime.Now:dd.MM.yyyy HH:mm} - {file}";
            await DialogService.ShowMessageAsync("Резервная копия создана:\n" + file);
        }
        finally
        {
            IsBusy = false;
        }
    });
}
