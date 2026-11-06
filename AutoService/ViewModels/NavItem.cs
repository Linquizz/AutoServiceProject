namespace AutoService.ViewModels;

public record NavItem(string Title, Func<ViewModelBase> Create);
