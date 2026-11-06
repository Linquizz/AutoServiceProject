using System.Collections.ObjectModel;
using AutoService.Models;
using AutoService.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoService.ViewModels;

public partial class PartsViewModel : ViewModelBase
{
    private readonly CatalogService _catalog = new();

    public ObservableCollection<Part> Items { get; } = new();

    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private Part? _selected;

    [ObservableProperty] private string _articleText = "";
    [ObservableProperty] private string _nameText = "";
    [ObservableProperty] private string _priceText = "0";
    [ObservableProperty] private string _stockText = "0";

    public PartsViewModel()
    {
        _ = RefreshAsync();
    }

    private async Task RefreshAsync(string? selectArticle = null)
    {
        await RunAsync(async () =>
        {
            var keep = selectArticle ?? Selected?.Article;
            var list = await _catalog.GetPartsAsync(SearchText);
            Items.Clear();
            foreach (var p in list) Items.Add(p);
            Selected = Items.FirstOrDefault(p => p.Article == keep);
        });
    }

    partial void OnSelectedChanged(Part? value)
    {
        if (value == null) return;
        ArticleText = value.Article;
        NameText = value.Name;
        PriceText = value.Price.ToString("0.00");
        StockText = value.StockQuantity.ToString();
    }

    [RelayCommand]
    private Task SearchAsync() => RefreshAsync();

    [RelayCommand]
    private void ClearForm()
    {
        Selected = null;
        ArticleText = "";
        NameText = "";
        PriceText = "0";
        StockText = "0";
    }

    private Part ReadForm()
    {
        if (!ParseHelper.TryDecimal(PriceText, out var price))
            throw new BusinessException("Цена должна быть числом (например, 350 или 350,50).");
        if (!ParseHelper.TryInt(StockText, out var stock))
            throw new BusinessException("Остаток на складе должен быть целым числом.");

        return new Part
        {
            Article = ArticleText,
            Name = NameText,
            Price = price,
            StockQuantity = stock
        };
    }

    [RelayCommand]
    private async Task AddAsync() => await RunAsync(async () =>
    {
        var part = ReadForm();
        await _catalog.AddPartAsync(part);
        await RefreshAsync(part.Article);
    });

    [RelayCommand]
    private async Task SaveAsync() => await RunAsync(async () =>
    {
        if (Selected == null)
        {
            await DialogService.ShowMessageAsync("Выберите запасную часть в таблице. Для новой используйте кнопку «Добавить как новую».");
            return;
        }
        var part = ReadForm();
        part.Article = Selected.Article;
        await _catalog.UpdatePartAsync(part);
        await RefreshAsync(part.Article);
    });

    [RelayCommand]
    private async Task DeleteAsync() => await RunAsync(async () =>
    {
        if (Selected == null) return;
        if (!await DialogService.ConfirmAsync($"Удалить запасную часть «{Selected.Name}»?")) return;
        await _catalog.DeletePartAsync(Selected.Article);
        ClearForm();
        await RefreshAsync("-");
    });
}
