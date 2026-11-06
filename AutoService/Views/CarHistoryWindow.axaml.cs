using AutoService.ViewModels;
using Avalonia.Controls;

namespace AutoService.Views;

public partial class CarHistoryWindow : Window
{
    public CarHistoryWindow()
    {
        InitializeComponent();
        UiHelpers.FitToScreen(this, 980, 700, 800, 560);
        UiHelpers.EnableGridDeselect(this);
    }

    public CarHistoryWindow(CarHistoryViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += result => Close(result);
        Opened += async (_, _) => await viewModel.LoadAsync();
    }
}
