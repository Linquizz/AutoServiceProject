using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoService.Models;

[Table("Employee")]
public class Employee
{
    [Key, Column("ID_Employee")]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string LastName { get; set; } = "";

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = "";

    [MaxLength(50)]
    public string? MiddleName { get; set; }

    [Required, MaxLength(20)]
    public string Phone { get; set; } = "";

    [Required, MaxLength(50)]
    public string Position { get; set; } = "";

    [MaxLength(50)]
    public string? Login { get; set; }

    [MaxLength(200)]
    public string? PasswordHash { get; set; }

    [Required, MaxLength(20)]
    public string Role { get; set; } = Roles.Employee;

    [NotMapped]
    public string FullName => string.Join(" ",
        new[] { LastName, FirstName, MiddleName }.Where(s => !string.IsNullOrWhiteSpace(s)));

    [NotMapped]
    public string ShortName
    {
        get
        {
            var s = LastName;
            if (!string.IsNullOrWhiteSpace(FirstName)) s += $" {FirstName[0]}.";
            if (!string.IsNullOrWhiteSpace(MiddleName)) s += $" {MiddleName![0]}.";
            return s;
        }
    }

    public override string ToString() => $"{ShortName} ({Position})";
}
