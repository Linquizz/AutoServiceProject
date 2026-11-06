using System.Collections.ObjectModel;
using AutoService.Models;
using AutoService.Services;
using AutoService.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoService.ViewModels;

public partial class ClientsViewModel : ViewModelBase
{
    private readonly ClientService _service = new();

    public ObservableCollection<Client> Clients { get; } = new();

    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private Client? _selectedClient;
    [ObservableProperty] private string _summary = "";

    public ClientsViewModel()
    {
        _ = RefreshAsync();
    }

    private async Task RefreshAsync(int? selectId = null)
    {
        await RunAsync(async () =>
        {
            var keep = selectId ?? SelectedClient?.Id;
            var list = await _service.GetAllAsync(SearchText);

            Clients.Clear();
            foreach (var c in list) Clients.Add(c);

            SelectedClient = Clients.FirstOrDefault(c => c.Id == keep);
            Summary = list.Count == 0 ? "Клиенты не найдены." : $"Найдено клиентов: {list.Count}";
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
        var vm = new ClientEditViewModel(null);
        if (await DialogService.ShowDialogAsync(new ClientEditWindow(vm)))
            await RefreshAsync(vm.SavedClientId);
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (SelectedClient == null) return;
        var vm = new ClientEditViewModel(SelectedClient);
        if (await DialogService.ShowDialogAsync(new ClientEditWindow(vm)))
            await RefreshAsync(vm.SavedClientId);
    }

    [RelayCommand]
    private async Task DeleteAsync() => await RunAsync(async () =>
    {
        if (SelectedClient == null) return;
        var client = SelectedClient;

        var (cars, orders) = await _service.GetDeleteInfoAsync(client.Id);
        var text = $"Удалить клиента «{client.FullName}»?";
        if (cars > 0 || orders > 0)
            text += $"\n\nВместе с ним будут удалены автомобили ({cars}) и заказ-наряды ({orders}). Действие необратимо.";

        if (!await DialogService.ConfirmAsync(text, "Удаление клиента")) return;

        await _service.DeleteAsync(client.Id);
        await RefreshAsync(-1);
    });

    [RelayCommand]
    private async Task AddCarAsync()
    {
        if (SelectedClient == null)
        {
            await DialogService.ShowMessageAsync("Сначала выберите клиента в таблице.");
            return;
        }

        var vm = new CarEditViewModel(null, SelectedClient);
        if (await DialogService.ShowDialogAsync(new CarEditWindow(vm)))
            await RefreshAsync(SelectedClient.Id);
    }
}
