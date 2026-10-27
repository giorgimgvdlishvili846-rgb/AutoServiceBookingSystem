using AutoServiceBookingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBookingSystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Service> Services { get; set; } = null!;
    public DbSet<Vehicle> Vehicles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(user => user.Username).IsUnique();
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Username).UseCollation("NOCASE");
            entity.Property(user => user.Email).UseCollation("NOCASE");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasIndex(vehicle => vehicle.LicensePlate).IsUnique();
            entity.Property(vehicle => vehicle.LicensePlate).UseCollation("NOCASE");
            entity.HasOne(vehicle => vehicle.User)
                .WithMany(user => user.Vehicles)
                .HasForeignKey(vehicle => vehicle.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.Property(service => service.Price).HasPrecision(10, 2);
            entity.HasIndex(service => service.Name).IsUnique();
            entity.Property(service => service.Name).UseCollation("NOCASE");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasIndex(booking => new { booking.StartTime, booking.EndTime });
            entity.HasOne(booking => booking.User)
                .WithMany(user => user.Bookings)
                .HasForeignKey(booking => booking.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(booking => booking.Vehicle)
                .WithMany(vehicle => vehicle.Bookings)
                .HasForeignKey(booking => booking.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(booking => booking.Service)
                .WithMany(service => service.Bookings)
                .HasForeignKey(booking => booking.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}