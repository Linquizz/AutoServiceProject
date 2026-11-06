using System.Collections.ObjectModel;
using AutoService.Models;
using AutoService.Services;
using AutoService.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoService.ViewModels;

public partial class OrdersViewModel : ViewModelBase
{
    private readonly WorkOrderService _service = new();
    private readonly WorkOrderDocumentService _documents = new();

    public ObservableCollection<WorkOrder> Orders { get; } = new();
    public string[] StatusOptions { get; } = { "Все", "В работе", "Завершён" };

    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private string _selectedStatus = "Все";
    [ObservableProperty] private DateTime? _dateFrom;
    [ObservableProperty] private DateTime? _dateTo;
    [ObservableProperty] private WorkOrder? _selectedOrder;
    [ObservableProperty] private string _summary = "";

    public OrdersViewModel()
    {
        _ = RefreshAsync();
    }

    private async Task RefreshAsync(int? selectId = null)
    {
        await RunAsync(async () =>
        {
            var keep = selectId ?? SelectedOrder?.Id;
            bool? completed = SelectedStatus switch
            {
                "В работе" => false,
                "Завершён" => true,
                _ => null
            };

            var list = await _service.SearchAsync(SearchText, completed, DateFrom, DateTo);

            Orders.Clear();
            foreach (var o in list) Orders.Add(o);
            SelectedOrder = Orders.FirstOrDefault(o => o.Id == keep);

            Summary = list.Count == 0
                ? "Заказ-наряды не найдены."
                : $"Найдено: {list.Count}, на сумму {ParseHelper.Money(list.Sum(o => o.Total))} руб.";
        });
    }

    [RelayCommand]
    private Task SearchAsync() => RefreshAsync();

    [RelayCommand]
    private async Task ResetAsync()
    {
        SearchText = "";
        SelectedStatus = "Все";
        DateFrom = null;
        DateTo = null;
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task CreateAsync() => await OpenEditorAsync(null);

    [RelayCommand]
    private async Task OpenAsync()
    {
        if (SelectedOrder != null) await OpenEditorAsync(SelectedOrder.Id);
    }

    private async Task OpenEditorAsync(int? orderId)
    {
        var vm = new OrderEditViewModel(orderId, null);
        await DialogService.ShowDialogAsync(new OrderEditWindow(vm));
        await RefreshAsync(vm.CurrentOrderId > 0 ? vm.CurrentOrderId : orderId);
    }

    [RelayCommand]
    private async Task ToggleStatusAsync() => await RunAsync(async () =>
    {
        if (SelectedOrder == null) return;
        var order = SelectedOrder;

        if (!order.IsCompleted)
        {
            if (!await DialogService.ConfirmAsync(
                    $"Завершить работы по заказ-наряду № {order.Id}?\n\nБудет сформирован PDF, а если у клиента указана почта - отправлен ему."))
                return;

            var result = await _documents.CompleteAndSendAsync(order.Id);
            await DialogService.ShowMessageAsync(result.Message);
            FileLauncher.Open(result.PdfPath);
        }
        else
        {
            if (!await DialogService.ConfirmAsync($"Вернуть заказ-наряд № {order.Id} в статус «В работе»?")) return;
            await _service.SetCompletedAsync(order.Id, false);
        }

        await RefreshAsync(order.Id);
    });

    [RelayCommand]
    private async Task HistoryAsync() => await RunAsync(async () =>
    {
        if (SelectedOrder?.Car == null) return;
        await DialogService.ShowDialogAsync(new CarHistoryWindow(new CarHistoryViewModel(SelectedOrder.Car)));
    });
}
