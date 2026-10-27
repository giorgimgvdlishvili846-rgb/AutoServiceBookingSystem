using AutoServiceBookingSystem.DTOs;
using AutoServiceBookingSystem.Models;

namespace AutoServiceBookingSystem.Mappings;

public static class DtoMappings
{
    public static UserResponse ToResponse(this User user) =>
        new(user.Id, user.Username, user.Email, user.FirstName, user.LastName, user.Role, user.IsBlocked);

    public static VehicleResponse ToResponse(this Vehicle vehicle) =>
        new(vehicle.Id, vehicle.Make, vehicle.Model, vehicle.Year, vehicle.LicensePlate);

    public static ServiceResponse ToResponse(this Service service) =>
        new(service.Id, service.Name, service.Description, service.Price, service.DurationMinutes, service.IsActive);

    public static BookingResponse ToResponse(this Booking booking) =>
        new(
            booking.Id,
            booking.UserId,
            $"{booking.User.FirstName} {booking.User.LastName}".Trim(),
            booking.VehicleId,
            $"{booking.Vehicle.Make} {booking.Vehicle.Model}",
            booking.Vehicle.LicensePlate,
            booking.ServiceId,
            booking.Service.Name,
            booking.Service.Price,
            booking.StartTime,
            booking.EndTime,
            booking.Status,
            booking.CustomerNote,
            booking.AdminComment,
            booking.CreatedAt);
}
