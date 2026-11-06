using AutoService.Models;
using AutoService.Services;
using AutoService.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace AutoService.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel = new();

    public LoginWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        _viewModel.LoginSucceeded += OnLoginSucceeded;
        Opened += async (_, _) => await _viewModel.InitializeAsync();
    }

    private void OnLoginSucceeded(Employee employee)
    {
        Session.User = employee;

        var main = new MainWindow();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = main;

        main.Show();
        Close();
    }
}
