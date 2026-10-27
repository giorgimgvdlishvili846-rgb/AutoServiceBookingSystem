using AutoServiceBookingSystem.Models.Enums;

namespace AutoServiceBookingSystem.Models;

public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ServiceId { get; set; }
    public int VehicleId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CustomerNote { get;set; } 
    public string? AdminComment { get; set; } 
    public User User { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
    public Service Service { get; set; } = null!;


}