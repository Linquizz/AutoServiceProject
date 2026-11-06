using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoService.Models;

[Table("Brand")]
public class Brand
{
    [Key, Column("ID_Brand")]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = "";

    public List<CarModel> Models { get; set; } = new();

    public override string ToString() => Name;
}
