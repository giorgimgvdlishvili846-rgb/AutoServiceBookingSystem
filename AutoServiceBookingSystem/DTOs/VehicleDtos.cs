namespace AutoServiceBookingSystem.DTOs;

public sealed record VehicleRequest(
    string Make,
    string Model,
    int Year,
    string LicensePlate);

public sealed record VehicleResponse(
    int Id,
    string Make,
    string Model,
    int Year,
    string LicensePlate);
