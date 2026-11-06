using AutoService.ViewModels;
using Avalonia.Controls;

namespace AutoService.Views;

public partial class CarEditWindow : Window
{
    public CarEditWindow()
    {
        InitializeComponent();
        UiHelpers.LimitToScreen(this);
    }

    public CarEditWindow(CarEditViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += result => Close(result);
        Opened += async (_, _) => await viewModel.LoadAsync();
    }
}
