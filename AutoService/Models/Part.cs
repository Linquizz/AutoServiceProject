using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoService.Models;

[Table("Part")]
public class Part
{
    [Key, MaxLength(50), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Article { get; set; } = "";

    [Required, MaxLength(200)]
    public string Name { get; set; } = "";

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public override string ToString() => $"{Article} — {Name}";
}
