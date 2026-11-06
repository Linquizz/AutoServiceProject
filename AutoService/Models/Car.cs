using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoService.Models;

[Table("Car")]
public class Car
{
    [Key, MaxLength(10), DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string RegistrationNumber { get; set; } = "";

    [Column("ID_Client")]
    public int ClientId { get; set; }

    [Column("ID_Model")]
    public int ModelId { get; set; }

    [MaxLength(17)]
    public string? VIN { get; set; }

    public int Year { get; set; }

    [Required, MaxLength(15)]
    public string EngineType { get; set; } = "";

    public int Mileage { get; set; }

    public Client? Client { get; set; }
    public CarModel? Model { get; set; }
    public List<WorkOrder> WorkOrders { get; set; } = new();

    [NotMapped]
    public string Title => $"{Model?.Brand?.Name} {Model?.Name}".Trim();

    [NotMapped]
    public string Display => string.IsNullOrEmpty(Title) ? RegistrationNumber : $"{RegistrationNumber} — {Title}";

    public override string ToString() => Display;
}
