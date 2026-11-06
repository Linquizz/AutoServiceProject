using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoService.Models;

[Table("OrderService")]
public class OrderService
{
    [Key, Column("ID_OrderService")]
    public int Id { get; set; }

    [Column("ID_WorkOrder")]
    public int WorkOrderId { get; set; }

    [Column("ID_Service")]
    public int ServiceId { get; set; }

    [Column("ID_Employee")]
    public int EmployeeId { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? Duration { get; set; }

    public WorkOrder? WorkOrder { get; set; }
    public Service? Service { get; set; }
    public Employee? Employee { get; set; }

    [NotMapped] public decimal Sum => Price;
    [NotMapped] public string ServiceName => Service?.Name ?? "";
    [NotMapped] public string ExecutorName => Employee?.ShortName ?? "";
}
