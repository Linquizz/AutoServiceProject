namespace AutoService.ViewModels;

public class AdminViewModel : ViewModelBase
{
    public EmployeesViewModel Employees { get; } = new();
    public BackupViewModel Backup { get; } = new();
}
