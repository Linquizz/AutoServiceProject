using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoService.Models;

[Table("WorkOrder")]
public class WorkOrder
{
    [Key, Column("ID_WorkOrder")]
    public int Id { get; set; }

    [Required, MaxLength(10)]
    public string RegistrationNumber { get; set; } = "";

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    [Column("Status")]
    public bool IsCompleted { get; set; }

    public Car? Car { get; set; }
    public List<OrderService> Works { get; set; } = new();
    public List<OrderPart> Parts { get; set; } = new();

    [NotMapped] public decimal WorksTotal => Works.Sum(w => w.Sum);
    [NotMapped] public decimal PartsTotal => Parts.Sum(p => p.Sum);
    [NotMapped] public decimal Total => WorksTotal + PartsTotal;

    [NotMapped] public string StatusText => IsCompleted ? "Завершён" : "В работе";
    [NotMapped] public string ClientName => Car?.Client?.FullName ?? "";
    [NotMapped] public string CarTitle => Car?.Title ?? "";
}
