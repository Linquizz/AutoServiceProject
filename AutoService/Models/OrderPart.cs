using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoService.Models;

[Table("OrderPart")]
public class OrderPart
{
    [Key, Column("ID_OrderPart")]
    public int Id { get; set; }

    [Column("ID_WorkOrder")]
    public int WorkOrderId { get; set; }

    [Required, MaxLength(50)]
    public string Article { get; set; } = "";

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    public WorkOrder? WorkOrder { get; set; }
    public Part? Part { get; set; }

    [NotMapped] public decimal Sum => Price * Quantity;
    [NotMapped] public string PartName => Part?.Name ?? "";
}
