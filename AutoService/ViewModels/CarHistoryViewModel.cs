using System.Collections.ObjectModel;
using AutoService.Models;
using AutoService.Services;
using AutoService.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoService.ViewModels;

public partial class CarHistoryViewModel : DialogViewModelBase
{
    private readonly WorkOrderService _orders = new();
    private readonly Car _car;

    public string CarInfo { get; }
    public string OwnerInfo { get; }

    public ObservableCollection<WorkOrder> Orders { get; } = new();

    [ObservableProperty] private WorkOrder? _selectedOrder;
    [ObservableProperty] private string _summary = "";

    public CarHistoryViewModel(Car car)
    {
        _car = car;
        CarInfo = $"{car.RegistrationNumber} - {car.Title}, {car.Year} г., двигатель: {car.EngineType}, пробег: {car.Mileage} км" +
                  (string.IsNullOrEmpty(car.VIN) ? "" : $", VIN: {car.VIN}");
        OwnerInfo = car.Client == null ? "" : $"Владелец: {car.Client.FullName}, тел.: {car.Client.Phone}";
    }

    public async Task LoadAsync() => await RunAsync(async () =>
    {
        var list = await _orders.GetByCarAsync(_car.RegistrationNumber);
        Orders.Clear();
        foreach (var o in list) Orders.Add(o);

        SelectedOrder = Orders.FirstOrDefault();
        Summary = list.Count == 0
            ? "По этому автомобилю заказ-нарядов ещё нет."
            : $"Заказ-нарядов: {list.Count}, на сумму {ParseHelper.Money(list.Sum(o => o.Total))} руб.";
    });

    [RelayCommand]
    private async Task OpenOrderAsync()
    {
        if (SelectedOrder == null) return;
        await DialogService.ShowDialogAsync(new OrderEditWindow(new OrderEditViewModel(SelectedOrder.Id, null)));
        await LoadAsync();
    }

    [RelayCommand]
    private void CloseWindow() => Close(true);
}
