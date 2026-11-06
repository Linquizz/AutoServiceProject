using AutoService.Data;
using AutoService.Models;
using AutoService.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoService.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly AuthService _auth = new();

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SignInCommand))]
    private string _login = "";

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SignInCommand))]
    private string _password = "";

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SignInCommand))]
    private bool _isBusy;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = "";

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [ObservableProperty]
    private string _statusMessage = "";

    private bool _isDatabaseReady;

    public event Action<Employee>? LoginSucceeded;

    public async Task InitializeAsync()
    {
        IsBusy = true;
        ErrorMessage = AppSettings.LoadError ?? "";
        StatusMessage = "Подключение к базе данных...";
        try
        {
            await Task.Run(DbInitializer.Initialize);
            _isDatabaseReady = true;
            StatusMessage = "";
        }
        catch (Exception ex)
        {
            StatusMessage = "";
            ErrorMessage = "Не удалось подготовить базу данных.\n" + ErrorFormatter.Format(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSignIn() => !IsBusy && !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrEmpty(Password);

    [RelayCommand(CanExecute = nameof(CanSignIn))]
    private async Task SignInAsync()
    {
        ErrorMessage = "";

        if (!_isDatabaseReady)
        {
            await InitializeAsync();
            if (!_isDatabaseReady) return;
        }

        IsBusy = true;
        try
        {
            var employee = await _auth.LoginAsync(Login, Password);
            if (employee == null)
            {
                ErrorMessage = "Неверный логин или пароль.";
                Password = "";
                return;
            }
            LoginSucceeded?.Invoke(employee);
        }
        catch (Exception ex)
        {
            ErrorMessage = ErrorFormatter.Format(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
