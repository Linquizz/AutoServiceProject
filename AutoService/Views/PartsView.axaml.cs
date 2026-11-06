using Avalonia.Controls;

namespace AutoService.Views;

public partial class PartsView : UserControl
{
    public PartsView()
    {
        InitializeComponent();
        UiHelpers.EnableGridDeselect(this);
    }
}
