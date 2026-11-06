using AutoService.ViewModels;
using Avalonia.Controls;

namespace AutoService.Views;

public partial class AddWorkWindow : Window
{
    public AddWorkWindow()
    {
        InitializeComponent();
        UiHelpers.FitToScreen(this, 780, 600, 620, 480);
        UiHelpers.EnableGridDeselect(this);
    }

    public AddWorkWindow(AddWorkViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += result => Close(result);
        Opened += async (_, _) => await viewModel.LoadAsync();
    }
}
