using Avalonia.Controls;

namespace AutoService.Views;

public partial class ServicesView : UserControl
{
    public ServicesView()
    {
        InitializeComponent();
        UiHelpers.EnableGridDeselect(this);
    }
}
