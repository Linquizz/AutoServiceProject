using AutoService.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace AutoService.Services;

public static class DialogService
{
    public static Window? TopWindow =>
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?
            .Windows.LastOrDefault(w => w.IsVisible);

    public static Task ShowMessageAsync(string text, string title = "Автосервис") =>
        ShowDialogAsync(new MessageWindow(title, text, confirm: false));

    public static Task ShowErrorAsync(string text) =>
        ShowDialogAsync(new MessageWindow("Ошибка", text, confirm: false));

    public static Task<bool> ConfirmAsync(string text, string title = "Подтверждение") =>
        ShowDialogAsync(new MessageWindow(title, text, confirm: true));

    public static async Task<bool> ShowDialogAsync(Window dialog)
    {
        var owner = TopWindow;
        if (owner == null)
        {
            dialog.Show();
            return false;
        }
        return await dialog.ShowDialog<bool>(owner);
    }

    public static async Task<string?> PickFolderAsync(string title)
    {
        var owner = TopWindow;
        if (owner == null) return null;

        var folders = await owner.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });
        return folders.Count > 0 ? folders[0].Path.LocalPath : null;
    }

    public static async Task<string?> PickSavePdfAsync(string suggestedFileName)
    {
        var owner = TopWindow;
        if (owner == null) return null;

        var file = await owner.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Сохранить PDF",
            SuggestedFileName = suggestedFileName,
            DefaultExtension = "pdf",
            FileTypeChoices = new[] { new FilePickerFileType("PDF-документ") { Patterns = new[] { "*.pdf" } } }
        });
        return file?.Path.LocalPath;
    }
}
