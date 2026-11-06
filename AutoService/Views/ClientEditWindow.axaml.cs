using AutoService.ViewModels;
using Avalonia.Controls;

namespace AutoService.Views;

public partial class ClientEditWindow : Window
{
    public ClientEditWindow()
    {
        InitializeComponent();
        UiHelpers.LimitToScreen(this);
    }

    public ClientEditWindow(ClientEditViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += result => Close(result);
    }
}
