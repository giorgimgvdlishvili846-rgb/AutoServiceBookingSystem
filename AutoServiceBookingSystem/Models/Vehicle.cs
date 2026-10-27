namespace AutoServiceBookingSystem.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}