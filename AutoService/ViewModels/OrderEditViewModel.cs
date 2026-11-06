using System.Collections.ObjectModel;
using AutoService.Models;
using AutoService.Services;
using AutoService.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoService.ViewModels;

public partial class OrderEditViewModel : DialogViewModelBase
{
    private readonly WorkOrderService _orders = new();
    private readonly ClientService _clients = new();
    private readonly WorkOrderDocumentService _documents = new();
    private readonly int? _orderId;
    private readonly Car? _presetCar;

    public ObservableCollection<Client> Clients { get; } = new();
    public ObservableCollection<Car> Cars { get; } = new();
    public ObservableCollection<OrderService> Works { get; } = new();
    public ObservableCollection<OrderPart> Parts { get; } = new();

    [ObservableProperty] private string _title = "Новый заказ-наряд";

    [ObservableProperty] private Client? _selectedClient;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(CarInfo))]
    private Car? _selectedCar;

    [ObservableProperty] private string _reason = "";

    [ObservableProperty] private string _startDateText = "будет зафиксирована при сохранении";
    [ObservableProperty] private string _endDateText = "-";
    [ObservableProperty] private string _statusText = "Новый";

    [ObservableProperty] private OrderService? _selectedWork;
    [ObservableProperty] private OrderPart? _selectedPart;

    [ObservableProperty] private string _totalWorks = "0,00";
    [ObservableProperty] private string _totalParts = "0,00";
    [ObservableProperty] private string _totalAll = "0,00";
    [ObservableProperty] private string _emailInfo = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSaved), nameof(CanChangeCar), nameof(CanEditStructure), nameof(CanComplete), nameof(CanReopen))]
    private int _currentOrderId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEditHeader), nameof(CanEditStructure), nameof(CanComplete), nameof(CanReopen))]
    private bool _isCompleted;

    public bool IsSaved => CurrentOrderId > 0;
    public bool CanChangeCar => !IsSaved;
    public bool CanEditHeader => !IsCompleted;
    public bool CanEditStructure => IsSaved && !IsCompleted;
    public bool CanComplete => IsSaved && !IsCompleted;
    public bool CanReopen => IsSaved && IsCompleted;

    public string CarInfo => SelectedCar == null
        ? ""
        : $"{SelectedCar.Year} г., двигатель: {SelectedCar.EngineType}, пробег: {SelectedCar.Mileage} км" +
          (string.IsNullOrEmpty(SelectedCar.VIN) ? "" : $", VIN: {SelectedCar.VIN}");

    public OrderEditViewModel(int? orderId, Car? presetCar)
    {
        _orderId = orderId;
        _presetCar = presetCar;
    }

    public async Task LoadAsync() => await RunAsync(async () =>
    {
        await LoadClientsAsync();

        if (_orderId.HasValue)
        {
            await LoadOrderAsync(_orderId.Value);
            return;
        }

        if (_presetCar != null)
        {
            SelectedClient = Clients.FirstOrDefault(c => c.Id == _presetCar.ClientId);
            SelectedCar = Cars.FirstOrDefault(c => c.RegistrationNumber == _presetCar.RegistrationNumber);
        }
    });

    private async Task LoadClientsAsync()
    {
        var list = await _clients.GetAllWithCarsAsync();
        Clients.Clear();
        foreach (var c in list) Clients.Add(c);
    }

    private async Task LoadOrderAsync(int id)
    {
        var order = await _orders.GetAsync(id) ?? throw new BusinessException("Заказ-наряд не найден.");

        CurrentOrderId = order.Id;
        IsCompleted = order.IsCompleted;

        SelectedClient = Clients.FirstOrDefault(c => c.Id == order.Car?.ClientId);
        SelectedCar = Cars.FirstOrDefault(c => c.RegistrationNumber == order.RegistrationNumber);

        Reason = order.Reason ?? "";

        Title = $"Заказ-наряд № {order.Id}";
        StartDateText = order.StartDate.ToString("dd.MM.yyyy HH:mm");
        EndDateText = order.EndDate?.ToString("dd.MM.yyyy HH:mm") ?? "-";
        StatusText = order.StatusText;

        ApplyItems(order);
    }

    private void ApplyItems(WorkOrder order)
    {
        Works.Clear();
        foreach (var w in order.Works.OrderBy(w => w.Id)) Works.Add(w);
        Parts.Clear();
        foreach (var p in order.Parts.OrderBy(p => p.Id)) Parts.Add(p);

        TotalWorks = ParseHelper.Money(order.WorksTotal);
        TotalParts = ParseHelper.Money(order.PartsTotal);
        TotalAll = ParseHelper.Money(order.Total);
    }

    private async Task ReloadItemsAsync()
    {
        var order = await _orders.GetAsync(CurrentOrderId);
        if (order != null) ApplyItems(order);
    }

    partial void OnSelectedClientChanged(Client? value)
    {
        Cars.Clear();
        if (value != null)
            foreach (var car in value.Cars.OrderBy(c => c.RegistrationNumber)) Cars.Add(car);

        SelectedCar = Cars.Count == 1 ? Cars[0] : null;

        EmailInfo = value == null ? ""
            : string.IsNullOrWhiteSpace(value.Email)
                ? "У клиента не указана почта - письмо с заказ-нарядом отправляться не будет."
                : $"Почта клиента: {value.Email} - после завершения работ заказ-наряд будет отправлен на неё.";
    }

    [RelayCommand]
    private async Task NewCarAsync() => await RunAsync(async () =>
    {
        if (SelectedClient == null)
        {
            await DialogService.ShowMessageAsync("Сначала выберите клиента.");
            return;
        }

        var clientId = SelectedClient.Id;
        var vm = new CarEditViewModel(null, SelectedClient);
        if (!await DialogService.ShowDialogAsync(new CarEditWindow(vm))) return;

        await LoadClientsAsync();
        SelectedClient = Clients.FirstOrDefault(c => c.Id == clientId);
        SelectedCar = Cars.FirstOrDefault(c => c.RegistrationNumber == vm.SavedRegistrationNumber);
    });

    private async Task<bool> SaveCoreAsync()
    {
        return await TryAsync(async () =>
        {
            if (SelectedCar == null) throw new BusinessException("Выберите автомобиль клиента.");

            var order = new WorkOrder
            {
                Id = CurrentOrderId,
                RegistrationNumber = SelectedCar.RegistrationNumber,
                Reason = Reason
            };

            if (!IsSaved)
            {
                var id = await _orders.CreateAsync(order);
                await LoadOrderAsync(id);
            }
            else if (!IsCompleted)
            {
                await _orders.UpdateHeaderAsync(order);
            }
        });
    }

    [RelayCommand]
    private async Task SaveAsync() => await SaveCoreAsync();

    [RelayCommand]
    private async Task AddWorkAsync() => await RunAsync(async () =>
    {
        var vm = new AddWorkViewModel(null);
        if (await DialogService.ShowDialogAsync(new AddWorkWindow(vm)) && vm.Result != null)
        {
            await _orders.AddWorkAsync(CurrentOrderId, vm.Result);
            await ReloadItemsAsync();
        }
    });

    [RelayCommand]
    private async Task EditWorkAsync() => await RunAsync(async () =>
    {
        if (SelectedWork == null) return;
        var vm = new AddWorkViewModel(SelectedWork);
        if (await DialogService.ShowDialogAsync(new AddWorkWindow(vm)) && vm.Result != null)
        {
            await _orders.UpdateWorkAsync(vm.Result);
            await ReloadItemsAsync();
        }
    });

    [RelayCommand]
    private async Task DeleteWorkAsync() => await RunAsync(async () =>
    {
        if (SelectedWork == null) return;
        if (!await DialogService.ConfirmAsync($"Удалить работу «{SelectedWork.ServiceName}» из заказ-наряда?")) return;
        await _orders.DeleteWorkAsync(SelectedWork.Id);
        await ReloadItemsAsync();
    });

    [RelayCommand]
    private async Task AddPartAsync() => await RunAsync(async () =>
    {
        var vm = new AddPartViewModel(null);
        if (await DialogService.ShowDialogAsync(new AddPartWindow(vm)))
        {
            await _orders.AddPartAsync(CurrentOrderId, vm.Article, vm.Quantity);
            await ReloadItemsAsync();
        }
    });

    [RelayCommand]
    private async Task EditPartAsync() => await RunAsync(async () =>
    {
        if (SelectedPart == null) return;
        var vm = new AddPartViewModel(SelectedPart);
        if (await DialogService.ShowDialogAsync(new AddPartWindow(vm)))
        {
            await _orders.UpdatePartAsync(SelectedPart.Id, vm.Quantity);
            await ReloadItemsAsync();
        }
    });

    [RelayCommand]
    private async Task DeletePartAsync() => await RunAsync(async () =>
    {
        if (SelectedPart == null) return;
        if (!await DialogService.ConfirmAsync($"Удалить «{SelectedPart.PartName}» из заказ-наряда? Запчасть вернётся на склад.")) return;
        await _orders.DeletePartAsync(SelectedPart.Id);
        await ReloadItemsAsync();
    });

    [RelayCommand]
    private async Task CompleteAsync() => await RunAsync(async () =>
    {
        if (!await SaveCoreAsync()) return;

        if (!await DialogService.ConfirmAsync(
                "Завершить работы по заказ-наряду?\n\nПосле завершения он станет доступен только для просмотра. " +
                "Будет сформирован PDF, а если у клиента указана почта - отправлен ему."))
            return;

        var result = await _documents.CompleteAndSendAsync(CurrentOrderId);
        await LoadOrderAsync(CurrentOrderId);
        await DialogService.ShowMessageAsync(result.Message);
        FileLauncher.Open(result.PdfPath);
    });

    [RelayCommand]
    private async Task ReopenAsync() => await RunAsync(async () =>
    {
        if (!await DialogService.ConfirmAsync("Вернуть заказ-наряд в статус «В работе»?")) return;
        await _orders.SetCompletedAsync(CurrentOrderId, false);
        await LoadOrderAsync(CurrentOrderId);
    });

    [RelayCommand]
    private async Task CreatePdfAsync() => await RunAsync(async () =>
    {
        if (!IsCompleted && !await SaveCoreAsync()) return;
        var path = await _documents.GeneratePdfAsync(CurrentOrderId);
        FileLauncher.Open(path);
    });

    [RelayCommand]
    private async Task SendEmailAsync() => await RunAsync(async () =>
    {
        if (!IsCompleted && !await SaveCoreAsync()) return;
        var address = await _documents.SendToClientAsync(CurrentOrderId);
        await DialogService.ShowMessageAsync($"Заказ-наряд № {CurrentOrderId} отправлен на {address}.");
    });

    [RelayCommand]
    private void CloseWindow() => Close(true);
}
