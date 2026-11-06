using AutoService.ViewModels;
using Avalonia.Controls;

namespace AutoService.Views;

public partial class OrdersView : UserControl
{
    public OrdersView()
    {
        InitializeComponent();
        UiHelpers.EnableGridDeselect(this);
        OrdersGrid.DoubleTapped += (_, _) => (DataContext as OrdersViewModel)?.OpenCommand.Execute(null);
    }
}
