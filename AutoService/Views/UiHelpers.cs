using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AutoService.Views;

public static class UiHelpers
{
    public static void FitToScreen(Window window, double desiredWidth, double desiredHeight,
        double minWidth = 720, double minHeight = 520)
    {
        var (availWidth, availHeight) = GetAvailableSize(window);

        window.MinWidth = Math.Min(minWidth, availWidth);
        window.MinHeight = Math.Min(minHeight, availHeight);
        window.Width = Math.Max(window.MinWidth, Math.Min(desiredWidth, availWidth));
        window.Height = Math.Max(window.MinHeight, Math.Min(desiredHeight, availHeight));
    }

    public static void LimitToScreen(Window window)
    {
        var (availWidth, availHeight) = GetAvailableSize(window);
        window.MaxWidth = availWidth;
        window.MaxHeight = availHeight;
    }

    private static (double Width, double Height) GetAvailableSize(Window window)
    {
        double width = 1600, height = 900;
        try
        {
            var screen = window.Screens.Primary;
            if (screen != null)
            {
                width = screen.WorkingArea.Width / screen.Scaling;
                height = screen.WorkingArea.Height / screen.Scaling;
            }
        }
        catch
        {
        }
        return (width * 0.95, height * 0.92);
    }

    public static void EnableGridDeselect(ILogical root)
    {
        foreach (var grid in root.GetLogicalDescendants().OfType<DataGrid>())
        {
            var target = grid;

            target.Tapped += (_, e) =>
            {
                if (e.Source is not Visual source) return;

                var onItem = source.FindAncestorOfType<DataGridRow>(true) != null
                             || source.FindAncestorOfType<DataGridColumnHeader>(true) != null
                             || source.FindAncestorOfType<ScrollBar>(true) != null;
                if (!onItem) target.SelectedItem = null;
            };

            target.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Escape) target.SelectedItem = null;
            };
        }
    }
}
