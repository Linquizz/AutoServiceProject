using AutoService.Models;
using AutoService.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoService.ViewModels;

public partial class ClientEditViewModel : DialogViewModelBase
{
    private readonly ClientService _service = new();
    private readonly int _clientId;

    public string Title { get; }

    [ObservableProperty] private string _lastName = "";
    [ObservableProperty] private string _firstName = "";
    [ObservableProperty] private string _middleName = "";
    [ObservableProperty] private string _phone = "";
    [ObservableProperty] private string _email = "";

    public int SavedClientId { get; private set; }

    public ClientEditViewModel(Client? client)
    {
        if (client == null)
        {
            Title = "Новый клиент";
            return;
        }

        _clientId = client.Id;
        Title = "Изменение данных клиента";
        LastName = client.LastName;
        FirstName = client.FirstName;
        MiddleName = client.MiddleName ?? "";
        Phone = client.Phone;
        Email = client.Email ?? "";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var client = new Client
        {
            Id = _clientId,
            LastName = LastName,
            FirstName = FirstName,
            MiddleName = MiddleName,
            Phone = Phone,
            Email = Email
        };

        var ok = await TryAsync(async () =>
        {
            if (_clientId == 0) SavedClientId = await _service.AddAsync(client);
            else
            {
                await _service.UpdateAsync(client);
                SavedClientId = _clientId;
            }
        });

        if (ok) Close(true);
    }

    [RelayCommand]
    private void Cancel() => Close(false);
}
