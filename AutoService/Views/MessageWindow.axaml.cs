using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AutoService.Views;

public partial class MessageWindow : Window
{
    public MessageWindow()
    {
        InitializeComponent();
        UiHelpers.LimitToScreen(this);
    }

    public MessageWindow(string title, string text, bool confirm) : this()
    {
        Title = title;
        MessageText.Text = text;
        NoButton.IsVisible = confirm;
        YesButton.Content = confirm ? "Да" : "OK";
        if (!confirm) NoButton.IsCancel = false;
    }

    private void OnYes(object? sender, RoutedEventArgs e) => Close(true);

    private void OnNo(object? sender, RoutedEventArgs e) => Close(false);
}
