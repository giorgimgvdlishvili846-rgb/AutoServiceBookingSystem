using AutoServiceBookingSystem.Models.Enums;

namespace AutoServiceBookingSystem.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public bool IsBlocked { get; set; }
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}