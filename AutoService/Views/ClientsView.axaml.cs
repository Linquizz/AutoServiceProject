using AutoService.ViewModels;
using Avalonia.Controls;

namespace AutoService.Views;

public partial class ClientsView : UserControl
{
    public ClientsView()
    {
        InitializeComponent();
        UiHelpers.EnableGridDeselect(this);
        ClientsGrid.DoubleTapped += (_, _) => (DataContext as ClientsViewModel)?.EditCommand.Execute(null);
    }
}
