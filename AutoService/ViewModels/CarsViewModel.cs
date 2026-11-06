using System.Collections.ObjectModel;
using AutoService.Models;
using AutoService.Services;
using AutoService.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoService.ViewModels;

public partial class CarsViewModel : ViewModelBase
{
    private readonly CarService _service = new();

    public ObservableCollection<Car> Cars { get; } = new();

    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private Car? _selectedCar;
    [ObservableProperty] private string _summary = "";

    public CarsViewModel()
    {
        _ = RefreshAsync();
    }

    private async Task RefreshAsync(string? selectPlate = null)
    {
        await RunAsync(async () =>
        {
            var keep = selectPlate ?? SelectedCar?.RegistrationNumber;
            var list = await _service.GetAllAsync(SearchText);

            Cars.Clear();
            foreach (var c in list) Cars.Add(c);

            SelectedCar = Cars.FirstOrDefault(c => c.RegistrationNumber == keep);
            Summary = list.Count == 0 ? "Автомобили не найдены." : $"Найдено автомобилей: {list.Count}";
        });
    }

    [RelayCommand]
    private Task SearchAsync() => RefreshAsync();

    [RelayCommand]
    private async Task ResetAsync()
    {
        SearchText = "";
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        var vm = new CarEditViewModel(null, null);
        if (await DialogService.ShowDialogAsync(new CarEditWindow(vm)))
            await RefreshAsync(vm.SavedRegistrationNumber);
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (SelectedCar == null) return;
        var vm = new CarEditViewModel(SelectedCar, null);
        if (await DialogService.ShowDialogAsync(new CarEditWindow(vm)))
            await RefreshAsync(vm.SavedRegistrationNumber);
    }

    [RelayCommand]
    private async Task HistoryAsync()
    {
        if (SelectedCar == null) return;
        await DialogService.ShowDialogAsync(new CarHistoryWindow(new CarHistoryViewModel(SelectedCar)));
    }

    [RelayCommand]
    private async Task CreateOrderAsync()
    {
        if (SelectedCar == null) return;
        var vm = new OrderEditViewModel(null, SelectedCar);
        await DialogService.ShowDialogAsync(new OrderEditWindow(vm));
    }
}
