using AutoServiceBookingSystem.DTOs;
using FluentValidation;

namespace AutoServiceBookingSystem.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Username)
            .NotEmpty().Length(3, 30)
            .Matches("^[a-zA-Z0-9._-]+$");
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(120);
        RuleFor(request => request.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(request => request.LastName).NotEmpty().MaximumLength(50);
        RuleFor(request => request.Password)
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a number.");
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Username).NotEmpty();
        RuleFor(request => request.Password).NotEmpty();
    }
}

public sealed class VehicleRequestValidator : AbstractValidator<VehicleRequest>
{
    public VehicleRequestValidator()
    {
        RuleFor(request => request.Make).NotEmpty().MaximumLength(50);
        RuleFor(request => request.Model).NotEmpty().MaximumLength(50);
        RuleFor(request => request.Year).InclusiveBetween(1950, DateTime.UtcNow.Year + 1);
        RuleFor(request => request.LicensePlate).NotEmpty().Length(2, 20);
    }
}

public sealed class ServiceRequestValidator : AbstractValidator<ServiceRequest>
{
    public ServiceRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(80);
        RuleFor(request => request.Description).MaximumLength(500);
        RuleFor(request => request.Price).GreaterThan(0).LessThanOrEqualTo(100_000);
        RuleFor(request => request.DurationMinutes).InclusiveBetween(15, 480);
    }
}

public sealed class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(request => request.VehicleId).GreaterThan(0);
        RuleFor(request => request.ServiceId).GreaterThan(0);
        RuleFor(request => request.StartTime).GreaterThan(DateTime.UtcNow);
        RuleFor(request => request.CustomerNote).MaximumLength(500);
    }
}

public sealed class UpdateBookingStatusRequestValidator : AbstractValidator<UpdateBookingStatusRequest>
{
    public UpdateBookingStatusRequestValidator()
    {
        RuleFor(request => request.Status).IsInEnum();
        RuleFor(request => request.AdminComment).MaximumLength(500);
    }
}
