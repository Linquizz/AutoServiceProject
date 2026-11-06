namespace AutoService.ViewModels;

public class ReferencesViewModel : ViewModelBase
{
    public BrandsViewModel Brands { get; } = new();
    public ServicesViewModel Services { get; } = new();
    public PartsViewModel Parts { get; } = new();
}
