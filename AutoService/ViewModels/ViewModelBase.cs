using AutoService.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoService.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    public bool IsAdmin => Session.IsAdmin;

    protected static async Task RunAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync(ErrorFormatter.Format(ex));
        }
    }
}

public abstract partial class DialogViewModelBase : ViewModelBase
{
    public event Action<bool>? CloseRequested;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = "";

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    protected void Close(bool result) => CloseRequested?.Invoke(result);

    protected async Task<bool> TryAsync(Func<Task> action)
    {
        ErrorMessage = "";
        try
        {
            await action();
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ErrorFormatter.Format(ex);
            return false;
        }
    }
}
