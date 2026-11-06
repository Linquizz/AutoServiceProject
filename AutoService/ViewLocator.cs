using AutoService.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace AutoService;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null) return null;

        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
            return (Control)Activator.CreateInstance(type)!;

        return new TextBlock { Text = "Представление не найдено: " + name };
    }

    public bool Match(object? data) => data is ViewModelBase;
}
