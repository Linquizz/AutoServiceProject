using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoService.Models;

[Table("Client")]
public class Client
{
    [Key, Column("ID_Client")]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string LastName { get; set; } = "";

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = "";

    [MaxLength(50)]
    public string? MiddleName { get; set; }

    [Required, MaxLength(20)]
    public string Phone { get; set; } = "";

    [MaxLength(50)]
    public string? Email { get; set; }

    public List<Car> Cars { get; set; } = new();

    [NotMapped]
    public string FullName => string.Join(" ",
        new[] { LastName, FirstName, MiddleName }.Where(s => !string.IsNullOrWhiteSpace(s)));

    [NotMapped]
    public string CarsCount => Cars.Count.ToString();

    public override string ToString() => FullName;
}
