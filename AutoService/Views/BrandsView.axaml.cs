using Avalonia.Controls;

namespace AutoService.Views;

public partial class BrandsView : UserControl
{
    public BrandsView()
    {
        InitializeComponent();
        UiHelpers.EnableGridDeselect(this);
    }
}
