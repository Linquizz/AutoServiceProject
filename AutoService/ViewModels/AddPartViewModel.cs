using System.Collections.ObjectModel;
using AutoService.Models;
using AutoService.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoService.ViewModels;

public partial class AddPartViewModel : DialogViewModelBase
{
    private readonly CatalogService _catalog = new();
    private readonly OrderPart? _editing;
    private bool _loading;

    public string Title { get; }
    public string ConfirmText => _editing == null ? "Добавить" : "Сохранить";
    public bool CanPickPart => _editing == null;

    public ObservableCollection<Part> Parts { get; } = new();

    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private Part? _selectedPart;
    [ObservableProperty] private string _quantityText = "1";
    [ObservableProperty] private string _stockInfo = "Выберите запасную часть в таблице";
    [ObservableProperty] private string _priceText = "-";
    [ObservableProperty] private string _sumText = "-";

    public string Article { get; private set; } = "";
    public int Quantity { get; private set; }

    public AddPartViewModel(OrderPart? editing)
    {
        _editing = editing;
        Title = editing == null ? "Добавление запасной части" : "Изменение количества";
    }

    private int MaxQuantity => SelectedPart == null ? 0 : SelectedPart.StockQuantity + (_editing?.Quantity ?? 0);

    public async Task LoadAsync() => await RunAsync(async () =>
    {
        await ReloadPartsAsync();

        if (_editing != null)
        {
            _loading = true;
            SelectedPart = Parts.FirstOrDefault(p => p.Article == _editing.Article);
            QuantityText = _editing.Quantity.ToString();
            _loading = false;
            Refresh();
        }
    });

    private async Task ReloadPartsAsync()
    {
        var list = await _catalog.GetPartsAsync(SearchText, onlyInStock: true, alwaysIncludeArticle: _editing?.Article);
        var selected = SelectedPart?.Article;

        Parts.Clear();
        foreach (var p in list) Parts.Add(p);
        if (selected != null) SelectedPart = Parts.FirstOrDefault(p => p.Article == selected);
    }

    partial void OnSearchTextChanged(string value)
    {
        if (!_loading) _ = RunAsync(ReloadPartsAsync);
    }

    partial void OnSelectedPartChanged(Part? value) => Refresh();
    partial void OnQuantityTextChanged(string value) => Refresh();

    private void Refresh()
    {
        if (_loading) return;

        if (SelectedPart == null)
        {
            StockInfo = "Выберите запасную часть в таблице";
            PriceText = "-";
            SumText = "-";
            return;
        }

        var price = _editing?.Price ?? SelectedPart.Price;
        PriceText = ParseHelper.Money(price);
        StockInfo = $"Остаток на складе: {MaxQuantity}. Допустимое количество: от 1 до {MaxQuantity}.";

        SumText = ParseHelper.TryInt(QuantityText, out var qty) ? ParseHelper.Money(price * qty) : "-";
    }

    [RelayCommand]
    private void Confirm()
    {
        ErrorMessage = "";

        if (SelectedPart == null) { ErrorMessage = "Выберите запасную часть в таблице."; return; }
        if (!ParseHelper.TryInt(QuantityText, out var qty) || qty < 1)
        { ErrorMessage = "Количество должно быть целым числом не менее 1."; return; }
        if (qty > MaxQuantity)
        { ErrorMessage = $"Недостаточно на складе: доступно {MaxQuantity} шт."; return; }

        Article = SelectedPart.Article;
        Quantity = qty;
        Close(true);
    }

    [RelayCommand]
    private void Cancel() => Close(false);
}
