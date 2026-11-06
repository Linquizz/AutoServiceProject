using System.Collections.ObjectModel;
using AutoService.Models;
using AutoService.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoService.ViewModels;

public partial class ServicesViewModel : ViewModelBase
{
    private readonly CatalogService _catalog = new();

    public ObservableCollection<Service> Items { get; } = new();

    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private Service? _selected;

    [ObservableProperty] private string _nameText = "";
    [ObservableProperty] private string _descriptionText = "";
    [ObservableProperty] private string _priceText = "0";

    public ServicesViewModel()
    {
        _ = RefreshAsync();
    }

    private async Task RefreshAsync(int? selectId = null)
    {
        await RunAsync(async () =>
        {
            var keep = selectId ?? Selected?.Id;
            var list = await _catalog.GetServicesAsync(SearchText);
            Items.Clear();
            foreach (var s in list) Items.Add(s);
            Selected = Items.FirstOrDefault(s => s.Id == keep);
        });
    }

    partial void OnSelectedChanged(Service? value)
    {
        if (value == null) return;
        NameText = value.Name;
        DescriptionText = value.Description ?? "";
        PriceText = value.Price.ToString("0.00");
    }

    [RelayCommand]
    private Task SearchAsync() => RefreshAsync();

    [RelayCommand]
    private void ClearForm()
    {
        Selected = null;
        NameText = "";
        DescriptionText = "";
        PriceText = "0";
    }

    private Service? ReadForm(int id)
    {
        if (!ParseHelper.TryDecimal(PriceText, out var price))
            throw new BusinessException("Стоимость должна быть числом (например, 1500 или 1500,50).");

        return new Service
        {
            Id = id,
            Name = NameText,
            Description = DescriptionText,
            Price = price
        };
    }

    [RelayCommand]
    private async Task AddAsync() => await RunAsync(async () =>
    {
        var service = ReadForm(0)!;
        await _catalog.SaveServiceAsync(service);
        await RefreshAsync(service.Id);
    });

    [RelayCommand]
    private async Task SaveAsync() => await RunAsync(async () =>
    {
        if (Selected == null)
        {
            await DialogService.ShowMessageAsync("Выберите услугу в таблице. Для новой услуги используйте кнопку «Добавить как новую».");
            return;
        }
        var service = ReadForm(Selected.Id)!;
        await _catalog.SaveServiceAsync(service);
        await RefreshAsync(service.Id);
    });

    [RelayCommand]
    private async Task DeleteAsync() => await RunAsync(async () =>
    {
        if (Selected == null) return;
        if (!await DialogService.ConfirmAsync($"Удалить услугу «{Selected.Name}»?")) return;
        await _catalog.DeleteServiceAsync(Selected.Id);
        ClearForm();
        await RefreshAsync(-1);
    });
}
