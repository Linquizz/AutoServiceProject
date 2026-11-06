namespace AutoService.Models;

public static class Roles
{
    public const string Admin = "Администратор";
    public const string Employee = "Сотрудник";

    public static readonly string[] All = { Admin, Employee };
}
