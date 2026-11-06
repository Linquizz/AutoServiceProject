using AutoService.Models;
using AutoService.Services;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Data;

public class AutoServiceDbContext : DbContext
{
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<CarModel> CarModels => Set<CarModel>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<OrderService> OrderServices => Set<OrderService>();
    public DbSet<OrderPart> OrderParts => Set<OrderPart>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer(AppSettings.Current.ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<CarModel>()
            .HasOne(m => m.Brand).WithMany(br => br.Models)
            .HasForeignKey(m => m.BrandId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Car>()
            .HasOne(c => c.Client).WithMany(cl => cl.Cars)
            .HasForeignKey(c => c.ClientId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<Car>()
            .HasOne(c => c.Model).WithMany(m => m.Cars)
            .HasForeignKey(c => c.ModelId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<WorkOrder>()
            .HasOne(w => w.Car).WithMany(c => c.WorkOrders)
            .HasForeignKey(w => w.RegistrationNumber).OnDelete(DeleteBehavior.Cascade);

        b.Entity<OrderService>()
            .HasOne(o => o.WorkOrder).WithMany(w => w.Works)
            .HasForeignKey(o => o.WorkOrderId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<OrderService>()
            .HasOne(o => o.Service).WithMany()
            .HasForeignKey(o => o.ServiceId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<OrderService>()
            .HasOne(o => o.Employee).WithMany()
            .HasForeignKey(o => o.EmployeeId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<OrderPart>()
            .HasOne(o => o.WorkOrder).WithMany(w => w.Parts)
            .HasForeignKey(o => o.WorkOrderId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<OrderPart>()
            .HasOne(o => o.Part).WithMany()
            .HasForeignKey(o => o.Article).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Brand>().HasIndex(x => x.Name).IsUnique();
        b.Entity<CarModel>().HasIndex(x => new { x.BrandId, x.Name }).IsUnique();
        b.Entity<Employee>().HasIndex(x => x.Login).IsUnique();
        b.Entity<Car>().HasIndex(x => x.ClientId);
        b.Entity<WorkOrder>().HasIndex(x => x.RegistrationNumber);
    }
}
