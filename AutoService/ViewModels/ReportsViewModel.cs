using System.Collections.ObjectModel;
using AutoService.Models;
using AutoService.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoService.ViewModels;

public partial class ReportsViewModel : ViewModelBase
{
    private readonly WorkOrderService _orders = new();
    private readonly WorkOrderDocumentService _documents = new();

    public ObservableCollection<WorkOrder> Orders { get; } = new();

    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private DateTime? _dateFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    [ObservableProperty] private DateTime? _dateTo;
    [ObservableProperty] private WorkOrder? _selectedOrder;
    [ObservableProperty] private string _recipientEmail = "";
    [ObservableProperty] private string _summary = "";
    [ObservableProperty] private string _statusMessage = "";

    public string PdfFolder => AppPaths.WorkOrdersFolder;

    public ReportsViewModel()
    {
        _ = RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        await RunAsync(async () =>
        {
            var keep = SelectedOrder?.Id;
            var list = await _orders.SearchAsync(SearchText, true, DateFrom, DateTo);

            Orders.Clear();
            foreach (var o in list) Orders.Add(o);
            SelectedOrder = Orders.FirstOrDefault(o => o.Id == keep);

            Summary = list.Count == 0
                ? "За выбранный период завершённых заказ-нарядов нет."
                : $"Завершённых заказ-нарядов: {list.Count}. Работы: {ParseHelper.Money(list.Sum(o => o.WorksTotal))} руб., " +
                  $"запчасти: {ParseHelper.Money(list.Sum(o => o.PartsTotal))} руб., выручка: {ParseHelper.Money(list.Sum(o => o.Total))} руб.";
        });
    }

    partial void OnSelectedOrderChanged(WorkOrder? value)
    {
        RecipientEmail = value?.Car?.Client?.Email ?? "";
        StatusMessage = "";
    }

    [RelayCommand]
    private Task SearchAsync() => RefreshAsync();

    [RelayCommand]
    private async Task ResetAsync()
    {
        SearchText = "";
        DateFrom = null;
        DateTo = null;
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task CreatePdfAsync() => await RunAsync(async () =>
    {
        if (SelectedOrder == null) return;
        var path = await _documents.GeneratePdfAsync(SelectedOrder.Id);
        StatusMessage = "PDF сформирован: " + path;
        FileLauncher.Open(path);
    });

    [RelayCommand]
    private async Task SaveAsAsync() => await RunAsync(async () =>
    {
        if (SelectedOrder == null) return;

        var target = await DialogService.PickSavePdfAsync($"ЗаказНаряд_{SelectedOrder.Id}.pdf");
        if (target == null) return;

        var source = await _documents.GeneratePdfAsync(SelectedOrder.Id);
        File.Copy(source, target, true);
        StatusMessage = "PDF сохранён: " + target;
    });

    [RelayCommand]
    private async Task SendAsync() => await RunAsync(async () =>
    {
        if (SelectedOrder == null) return;

        StatusMessage = "Отправка письма...";
        try
        {
            var address = await _documents.SendToClientAsync(SelectedOrder.Id, RecipientEmail);
            StatusMessage = $"Заказ-наряд № {SelectedOrder.Id} отправлен на {address}.";
            await DialogService.ShowMessageAsync(StatusMessage);
        }
        catch
        {
            StatusMessage = "";
            throw;
        }
    });

    [RelayCommand]
    private void OpenFolder() => FileLauncher.OpenFolder(PdfFolder);
}
