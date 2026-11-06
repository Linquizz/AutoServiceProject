using System.Collections.ObjectModel;
using AutoService.Models;
using AutoService.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoService.ViewModels;

public partial class EmployeesViewModel : ViewModelBase
{
    private readonly EmployeeService _service = new();

    public ObservableCollection<Employee> Employees { get; } = new();
    public string[] RoleOptions => Roles.All;

    [ObservableProperty] private Employee? _selected;

    [ObservableProperty] private string _lastName = "";
    [ObservableProperty] private string _firstName = "";
    [ObservableProperty] private string _middleName = "";
    [ObservableProperty] private string _phone = "";
    [ObservableProperty] private string _position = "";
    [ObservableProperty] private string _login = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private string? _role = Roles.Employee;

    public EmployeesViewModel()
    {
        _ = RefreshAsync();
    }

    private async Task RefreshAsync(int? selectId = null)
    {
        await RunAsync(async () =>
        {
            var keep = selectId ?? Selected?.Id;
            var list = await _service.GetAllAsync();
            Employees.Clear();
            foreach (var e in list) Employees.Add(e);
            Selected = Employees.FirstOrDefault(e => e.Id == keep);
        });
    }

    partial void OnSelectedChanged(Employee? value)
    {
        if (value == null) return;
        LastName = value.LastName;
        FirstName = value.FirstName;
        MiddleName = value.MiddleName ?? "";
        Phone = value.Phone;
        Position = value.Position;
        Login = value.Login ?? "";
        Role = value.Role;
        Password = "";
    }

    [RelayCommand]
    private void ClearForm()
    {
        Selected = null;
        LastName = FirstName = MiddleName = Phone = Position = Login = Password = "";
        Role = Roles.Employee;
    }

    private Employee ReadForm(int id) => new()
    {
        Id = id,
        LastName = LastName,
        FirstName = FirstName,
        MiddleName = MiddleName,
        Phone = Phone,
        Position = Position,
        Login = Login,
        Role = Role ?? ""
    };

    [RelayCommand]
    private async Task AddAsync() => await RunAsync(async () =>
    {
        var employee = ReadForm(0);
        await _service.AddAsync(employee, Password);
        await RefreshAsync(employee.Id);
        Password = "";
    });

    [RelayCommand]
    private async Task SaveAsync() => await RunAsync(async () =>
    {
        if (Selected == null)
        {
            await DialogService.ShowMessageAsync("Выберите сотрудника в таблице. Для нового используйте «Добавить как нового».");
            return;
        }
        var id = Selected.Id;
        await _service.UpdateAsync(ReadForm(id), Password);
        await RefreshAsync(id);
        Password = "";
    });

    [RelayCommand]
    private async Task DeleteAsync() => await RunAsync(async () =>
    {
        if (Selected == null) return;
        if (!await DialogService.ConfirmAsync($"Удалить сотрудника «{Selected.FullName}»?")) return;
        await _service.DeleteAsync(Selected.Id);
        ClearForm();
        await RefreshAsync(-1);
    });
}
