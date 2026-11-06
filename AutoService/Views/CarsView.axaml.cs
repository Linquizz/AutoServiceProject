using AutoService.ViewModels;
using Avalonia.Controls;

namespace AutoService.Views;

public partial class CarsView : UserControl
{
    public CarsView()
    {
        InitializeComponent();
        UiHelpers.EnableGridDeselect(this);
        CarsGrid.DoubleTapped += (_, _) => (DataContext as CarsViewModel)?.EditCommand.Execute(null);
    }
}
