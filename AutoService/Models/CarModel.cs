using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoService.Models;

[Table("Model")]
public class CarModel
{
    [Key, Column("ID_Model")]
    public int Id { get; set; }

    [Column("ID_Brand")]
    public int BrandId { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = "";

    public Brand? Brand { get; set; }
    public List<Car> Cars { get; set; } = new();

    public override string ToString() => Name;
}
