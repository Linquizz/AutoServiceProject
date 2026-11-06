using System.Collections.ObjectModel;
using AutoService.Models;
using AutoService.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoService.ViewModels;

public partial class BrandsViewModel : ViewModelBase
{
    private readonly CatalogService _catalog = new();

    public ObservableCollection<Brand> Brands { get; } = new();
    public ObservableCollection<CarModel> Models { get; } = new();

    [ObservableProperty] private Brand? _selectedBrand;
    [ObservableProperty] private CarModel? _selectedModel;
    [ObservableProperty] private string _brandName = "";
    [ObservableProperty] private string _modelName = "";

    public BrandsViewModel()
    {
        _ = RefreshAsync();
    }

    private async Task RefreshAsync(int? selectBrandId = null, int? selectModelId = null)
    {
        await RunAsync(async () =>
        {
            var keepBrand = selectBrandId ?? SelectedBrand?.Id;
            var list = await _catalog.GetBrandsAsync();
            Brands.Clear();
            foreach (var b in list) Brands.Add(b);

            SelectedBrand = Brands.FirstOrDefault(b => b.Id == keepBrand);
            if (selectModelId.HasValue)
                SelectedModel = Models.FirstOrDefault(m => m.Id == selectModelId);
        });
    }

    partial void OnSelectedBrandChanged(Brand? value)
    {
        Models.Clear();
        SelectedModel = null;
        BrandName = value?.Name ?? "";

        if (value == null) return;
        foreach (var m in value.Models) Models.Add(m);
    }

    partial void OnSelectedModelChanged(CarModel? value) => ModelName = value?.Name ?? "";

    [RelayCommand]
    private async Task AddBrandAsync() => await RunAsync(async () =>
    {
        var id = await _catalog.AddBrandAsync(BrandName);
        await RefreshAsync(id);
    });

    [RelayCommand]
    private async Task RenameBrandAsync() => await RunAsync(async () =>
    {
        if (SelectedBrand == null) return;
        var id = SelectedBrand.Id;
        await _catalog.RenameBrandAsync(id, BrandName);
        await RefreshAsync(id);
    });

    [RelayCommand]
    private async Task DeleteBrandAsync() => await RunAsync(async () =>
    {
        if (SelectedBrand == null) return;
        if (!await DialogService.ConfirmAsync($"Удалить марку «{SelectedBrand.Name}»?")) return;
        await _catalog.DeleteBrandAsync(SelectedBrand.Id);
        await RefreshAsync(-1);
    });

    [RelayCommand]
    private async Task AddModelAsync() => await RunAsync(async () =>
    {
        if (SelectedBrand == null)
        {
            await DialogService.ShowMessageAsync("Сначала выберите марку, к которой относится модель.");
            return;
        }
        var brandId = SelectedBrand.Id;
        await _catalog.AddModelAsync(brandId, ModelName);
        await RefreshAsync(brandId);
        ModelName = "";
    });

    [RelayCommand]
    private async Task RenameModelAsync() => await RunAsync(async () =>
    {
        if (SelectedModel == null || SelectedBrand == null) return;
        var brandId = SelectedBrand.Id;
        var modelId = SelectedModel.Id;
        await _catalog.RenameModelAsync(modelId, ModelName);
        await RefreshAsync(brandId, modelId);
    });

    [RelayCommand]
    private async Task DeleteModelAsync() => await RunAsync(async () =>
    {
        if (SelectedModel == null || SelectedBrand == null) return;
        if (!await DialogService.ConfirmAsync($"Удалить модель «{SelectedModel.Name}»?")) return;
        var brandId = SelectedBrand.Id;
        await _catalog.DeleteModelAsync(SelectedModel.Id);
        await RefreshAsync(brandId);
    });
}
