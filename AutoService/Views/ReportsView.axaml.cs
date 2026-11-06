using Avalonia.Controls;

namespace AutoService.Views;

public partial class ReportsView : UserControl
{
    public ReportsView()
    {
        InitializeComponent();
        UiHelpers.EnableGridDeselect(this);
    }
}
