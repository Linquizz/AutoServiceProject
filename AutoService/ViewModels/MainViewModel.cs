using System.Collections.ObjectModel;
using AutoService.Services;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoService.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ObservableCollection<NavItem> Items { get; } = new();

    [ObservableProperty]
    private NavItem? _selectedItem;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    public string UserName => Session.User?.ShortName ?? "";
    public string UserRole => Session.User?.Role ?? "";

    public event Action? LogoutRequested;

    public MainViewModel()
    {
        Items.Add(new NavItem("Заказ-наряды", () => new OrdersViewModel()));
        Items.Add(new NavItem("Клиенты", () => new ClientsViewModel()));
        Items.Add(new NavItem("Автомобили", () => new CarsViewModel()));
        Items.Add(new NavItem("Справочники", () => new ReferencesViewModel()));
        Items.Add(new NavItem("Отчёты", () => new ReportsViewModel()));
        if (Session.IsAdmin)
            Items.Add(new NavItem("Администрирование", () => new AdminViewModel()));

        SelectedItem = Items[0];
    }

    partial void OnSelectedItemChanged(NavItem? value) => CurrentPage = value?.Create();

    [RelayCommand]
    private void Logout() => LogoutRequested?.Invoke();

    [RelayCommand]
    private void ToggleTheme()
    {
        var app = Application.Current;
        if (app == null) return;
        app.RequestedThemeVariant = app.ActualThemeVariant == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
    }
}
