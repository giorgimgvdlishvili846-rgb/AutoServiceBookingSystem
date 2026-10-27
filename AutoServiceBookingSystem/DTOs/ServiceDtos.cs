namespace AutoServiceBookingSystem.DTOs;

public sealed record ServiceRequest(
    string Name,
    string Description,
    decimal Price,
    int DurationMinutes,
    bool IsActive = true);

public sealed record ServiceResponse(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int DurationMinutes,
    bool IsActive);
