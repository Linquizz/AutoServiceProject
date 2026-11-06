using Avalonia.Controls;

namespace AutoService.Views;

public partial class EmployeesView : UserControl
{
    public EmployeesView()
    {
        InitializeComponent();
        UiHelpers.EnableGridDeselect(this);
    }
}
