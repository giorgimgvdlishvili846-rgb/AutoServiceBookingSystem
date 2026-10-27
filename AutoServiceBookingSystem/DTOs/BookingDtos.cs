using AutoServiceBookingSystem.Models.Enums;

namespace AutoServiceBookingSystem.DTOs;

public sealed record CreateBookingRequest(
    int VehicleId,
    int ServiceId,
    DateTime StartTime,
    string? CustomerNote);

public sealed record UpdateBookingStatusRequest(
    BookingStatus Status,
    string? AdminComment);

public sealed record BookingResponse(
    int Id,
    int UserId,
    string CustomerName,
    int VehicleId,
    string Vehicle,
    string LicensePlate,
    int ServiceId,
    string ServiceName,
    decimal Price,
    DateTime StartTime,
    DateTime EndTime,
    BookingStatus Status,
    string? CustomerNote,
    string? AdminComment,
    DateTime CreatedAt);
