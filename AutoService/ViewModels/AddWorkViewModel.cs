using System.Collections.ObjectModel;
using AutoService.Models;
using AutoService.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoService.ViewModels;

public partial class AddWorkViewModel : DialogViewModelBase
{
    private readonly CatalogService _catalog = new();
    private readonly EmployeeService _employees = new();
    private readonly OrderService? _editing;
    private List<Service> _allServices = new();
    private bool _loading;

    public string Title { get; }
    public string ConfirmText => _editing == null ? "Добавить" : "Сохранить";

    public bool CanPickService => _editing == null;

    public ObservableCollection<Service> Services { get; } = new();
    public ObservableCollection<Employee> Employees { get; } = new();

    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private Service? _selectedService;
    [ObservableProperty] private Employee? _selectedEmployee;
    [ObservableProperty] private string _durationText = "";
    [ObservableProperty] private string _priceText = "-";

    public OrderService? Result { get; private set; }

    public AddWorkViewModel(OrderService? editing)
    {
        _editing = editing;
        Title = editing == null ? "Добавление услуги в заказ-наряд" : "Изменение работы";
    }

    public async Task LoadAsync() => await RunAsync(async () =>
    {
        _allServices = await _catalog.GetServicesAsync();
        foreach (var e in await _employees.GetAllAsync()) Employees.Add(e);
        ApplyFilter();

        if (_editing == null)
        {
            SelectedEmployee = Employees.FirstOrDefault(e => e.Id == Session.User?.Id) ?? Employees.FirstOrDefault();
            return;
        }

        _loading = true;
        SelectedService = _allServices.FirstOrDefault(s => s.Id == _editing.ServiceId);
        SelectedEmployee = Employees.FirstOrDefault(e => e.Id == _editing.EmployeeId);
        DurationText = _editing.Duration?.ToString("0.##") ?? "";
        PriceText = ParseHelper.Money(_editing.Price);
        _loading = false;
    });

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var t = SearchText.Trim();
        var filtered = _allServices.Where(s =>
            t.Length == 0 || s.Name.Contains(t, StringComparison.OrdinalIgnoreCase));

        var selectedId = SelectedService?.Id;
        Services.Clear();
        foreach (var s in filtered) Services.Add(s);
        if (selectedId.HasValue) SelectedService = Services.FirstOrDefault(s => s.Id == selectedId);
    }

    partial void OnSelectedServiceChanged(Service? value)
    {
        if (_loading) return;
        PriceText = value == null ? "-" : ParseHelper.Money(value.Price);
    }

    [RelayCommand]
    private void Confirm()
    {
        ErrorMessage = "";

        if (SelectedService == null) { ErrorMessage = "Выберите услугу в таблице."; return; }
        if (SelectedEmployee == null) { ErrorMessage = "Выберите исполнителя."; return; }

        decimal? duration = null;
        if (!string.IsNullOrWhiteSpace(DurationText))
        {
            if (!ParseHelper.TryDecimal(DurationText, out var d) || d < 0 || d > 999)
            {
                ErrorMessage = "Продолжительность должна быть неотрицательным числом часов (например, 1,5) или быть пустой.";
                return;
            }
            duration = d;
        }

        Result = new OrderService
        {
            Id = _editing?.Id ?? 0,
            ServiceId = SelectedService.Id,
            EmployeeId = SelectedEmployee.Id,
            Price = _editing?.Price ?? SelectedService.Price,
            Duration = duration
        };
        Close(true);
    }

    [RelayCommand]
    private void Cancel() => Close(false);
}
