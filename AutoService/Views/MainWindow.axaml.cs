using AutoService.Services;
using AutoService.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace AutoService.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        UiHelpers.FitToScreen(this, 1280, 780, 960, 600);
        DataContext = _viewModel;
        _viewModel.LogoutRequested += OnLogout;
    }

    private void OnLogout()
    {
        Session.User = null;

        var login = new LoginWindow();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = login;

        login.Show();
        Close();
    }
}
