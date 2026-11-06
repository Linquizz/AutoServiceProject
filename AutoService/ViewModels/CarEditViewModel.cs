using System.Collections.ObjectModel;
using AutoService.Models;
using AutoService.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoService.ViewModels;

public partial class CarEditViewModel : DialogViewModelBase
{
    private readonly CarService _cars = new();
    private readonly ClientService _clients = new();
    private readonly CatalogService _catalog = new();
    private readonly Car? _editing;
    private readonly Client? _presetOwner;

    public string Title { get; }
    public string[] EngineTypes => CarService.EngineTypes;

    public ObservableCollection<Client> Owners { get; } = new();
    public ObservableCollection<Brand> Brands { get; } = new();
    public ObservableCollection<CarModel> Models { get; } = new();

    [ObservableProperty] private Client? _selectedOwner;
    [ObservableProperty] private Brand? _selectedBrand;
    [ObservableProperty] private CarModel? _selectedModel;
    [ObservableProperty] private string _registrationNumber = "";
    [ObservableProperty] private string _vin = "";
    [ObservableProperty] private string _yearText = "";
    [ObservableProperty] private string? _selectedEngineType;
    [ObservableProperty] private string _mileageText = "0";

    public bool CanChangeOwner => _presetOwner == null;

    public bool CanEditPlate => _editing == null;

    public string? SavedRegistrationNumber { get; private set; }

    public CarEditViewModel(Car? car, Client? presetOwner)
    {
        _editing = car;
        _presetOwner = presetOwner;
        Title = car == null ? "Регистрация автомобиля" : "Изменение автомобиля";
        YearText = DateTime.Now.Year.ToString();
    }

    public async Task LoadAsync() => await RunAsync(async () =>
    {
        foreach (var c in await _clients.GetAllAsync()) Owners.Add(c);
        foreach (var b in await _catalog.GetBrandsAsync()) Brands.Add(b);

        if (_editing != null)
        {
            SelectedOwner = Owners.FirstOrDefault(o => o.Id == _editing.ClientId);
            RegistrationNumber = _editing.RegistrationNumber;
            Vin = _editing.VIN ?? "";
            YearText = _editing.Year.ToString();
            SelectedEngineType = _editing.EngineType;
            MileageText = _editing.Mileage.ToString();

            SelectedBrand = Brands.FirstOrDefault(b => b.Id == _editing.Model?.BrandId);
            SelectedModel = Models.FirstOrDefault(m => m.Id == _editing.ModelId);
        }
        else if (_presetOwner != null)
        {
            SelectedOwner = Owners.FirstOrDefault(o => o.Id == _presetOwner.Id);
        }
    });

    partial void OnSelectedBrandChanged(Brand? value)
    {
        Models.Clear();
        if (value != null)
            foreach (var m in value.Models) Models.Add(m);
        SelectedModel = null;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = "";

        if (!ParseHelper.TryInt(YearText, out var year))
        {
            ErrorMessage = "Год выпуска должен быть целым числом.";
            return;
        }
        if (!ParseHelper.TryInt(MileageText, out var mileage))
        {
            ErrorMessage = "Пробег должен быть целым неотрицательным числом.";
            return;
        }

        var car = new Car
        {
            RegistrationNumber = RegistrationNumber,
            ClientId = SelectedOwner?.Id ?? 0,
            ModelId = SelectedModel?.Id ?? 0,
            VIN = Vin,
            Year = year,
            EngineType = SelectedEngineType ?? "",
            Mileage = mileage
        };

        var ok = await TryAsync(async () =>
        {
            if (_editing == null) await _cars.AddAsync(car);
            else await _cars.UpdateAsync(car);
            SavedRegistrationNumber = Validator.NormalizePlate(RegistrationNumber);
        });

        if (ok) Close(true);
    }

    [RelayCommand]
    private void Cancel() => Close(false);
}
