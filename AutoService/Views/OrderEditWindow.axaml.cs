using AutoService.ViewModels;
using Avalonia.Controls;

namespace AutoService.Views;

public partial class OrderEditWindow : Window
{
    public OrderEditWindow()
    {
        InitializeComponent();
        UiHelpers.FitToScreen(this, 1100, 760, 860, 560);
        UiHelpers.EnableGridDeselect(this);
    }

    public OrderEditWindow(OrderEditViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += result => Close(result);
        Opened += async (_, _) => await viewModel.LoadAsync();
    }
}
