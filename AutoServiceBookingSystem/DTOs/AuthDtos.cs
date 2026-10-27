using AutoServiceBookingSystem.Models.Enums;

namespace AutoServiceBookingSystem.DTOs;

public sealed record RegisterRequest(
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Password);

public sealed record LoginRequest(string Username, string Password);

public sealed record UserResponse(
    int Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role,
    bool IsBlocked);

public sealed record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    UserResponse User);
