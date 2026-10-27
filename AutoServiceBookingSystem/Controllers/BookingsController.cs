using System.Security.Claims;
using AutoServiceBookingSystem.Data;
using AutoServiceBookingSystem.DTOs;
using AutoServiceBookingSystem.Mappings;
using AutoServiceBookingSystem.Models;
using AutoServiceBookingSystem.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBookingSystem.Controllers;

[ApiController]
[Authorize]
[Route("api/bookings")]
public sealed class BookingsController(ApplicationDbContext context) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = "Customer")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetMine()
    {
        var bookings = await WithDetails(context.Bookings)
            .Where(booking => booking.UserId == CurrentUserId)
            .OrderByDescending(booking => booking.StartTime)
            .ToListAsync();
        return Ok(bookings.Select(booking => booking.ToResponse()));
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingResponse>> GetMineById(int id)
    {
        var booking = await WithDetails(context.Bookings)
            .SingleOrDefaultAsync(booking => booking.Id == id && booking.UserId == CurrentUserId);
        return booking is null ? NotFound() : Ok(booking.ToResponse());
    }

    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create(CreateBookingRequest request)
    {
        var user = await context.Users.FindAsync(CurrentUserId);
        if (user is null)
            return Unauthorized();
        if (user.IsBlocked)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Blocked users cannot create bookings." });

        var vehicle = await context.Vehicles
            .SingleOrDefaultAsync(vehicle => vehicle.Id == request.VehicleId && vehicle.UserId == CurrentUserId);
        if (vehicle is null)
            return BadRequest(new { message = "The selected vehicle does not belong to you." });

        var service = await context.Services
            .SingleOrDefaultAsync(service => service.Id == request.ServiceId && service.IsActive);
        if (service is null)
            return BadRequest(new { message = "The selected service is not available." });

        var startUtc = request.StartTime.Kind == DateTimeKind.Utc
            ? request.StartTime
            : request.StartTime.ToUniversalTime();
        var endUtc = startUtc.AddMinutes(service.DurationMinutes);
        if (startUtc <= DateTime.UtcNow)
            return BadRequest(new { message = "Booking time must be in the future." });

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tbilisi");
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(startUtc, timeZone);
        var localEnd = TimeZoneInfo.ConvertTimeFromUtc(endUtc, timeZone);
        if (localStart.DayOfWeek == DayOfWeek.Sunday ||
            localStart.TimeOfDay < TimeSpan.FromHours(9) ||
            localEnd.TimeOfDay > TimeSpan.FromHours(18) ||
            localStart.Date != localEnd.Date)
        {
            return BadRequest(new { message = "Bookings must fit within Monday-Saturday, 09:00-18:00 (Tbilisi time)." });
        }

        var overlaps = await context.Bookings.AnyAsync(booking =>
            (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed) &&
            booking.StartTime < endUtc &&
            booking.EndTime > startUtc);
        if (overlaps)
            return Conflict(new { message = "The selected time overlaps another active booking." });

        var booking = new Booking
        {
            UserId = CurrentUserId,
            VehicleId = vehicle.Id,
            ServiceId = service.Id,
            StartTime = startUtc,
            EndTime = endUtc,
            CustomerNote = string.IsNullOrWhiteSpace(request.CustomerNote) ? null : request.CustomerNote.Trim(),
            User = user,
            Vehicle = vehicle,
            Service = service
        };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMineById), new { id = booking.Id }, booking.ToResponse());
    }

    [Authorize(Roles = "Customer")]
    [HttpPut("{id:int}/cancel")]
    public async Task<ActionResult<BookingResponse>> CancelMine(int id)
    {
        var booking = await WithDetails(context.Bookings)
            .SingleOrDefaultAsync(booking => booking.Id == id && booking.UserId == CurrentUserId);
        if (booking is null)
            return NotFound();
        if (booking.Status != BookingStatus.Pending)
            return Conflict(new { message = "Only pending bookings can be cancelled by a customer." });

        booking.Status = BookingStatus.Cancelled;
        await context.SaveChangesAsync();
        return Ok(booking.ToResponse());
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetAllForAdmin([FromQuery] BookingStatus? status)
    {
        var query = WithDetails(context.Bookings);
        if (status.HasValue)
            query = query.Where(booking => booking.Status == status);

        var bookings = await query.OrderByDescending(booking => booking.StartTime).ToListAsync();
        return Ok(bookings.Select(booking => booking.ToResponse()));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("admin/{id:int}/status")]
    public async Task<ActionResult<BookingResponse>> UpdateStatus(int id, UpdateBookingStatusRequest request)
    {
        var booking = await WithDetails(context.Bookings).SingleOrDefaultAsync(booking => booking.Id == id);
        if (booking is null)
            return NotFound();

        var allowed = booking.Status switch
        {
            BookingStatus.Pending => request.Status is BookingStatus.Confirmed or BookingStatus.Rejected or BookingStatus.Cancelled,
            BookingStatus.Confirmed => request.Status is BookingStatus.Completed or BookingStatus.Cancelled,
            _ => false
        };
        if (!allowed)
            return Conflict(new { message = $"Cannot change {booking.Status} booking to {request.Status}." });
        if (request.Status == BookingStatus.Rejected && string.IsNullOrWhiteSpace(request.AdminComment))
            return BadRequest(new { message = "Admin comment is required when rejecting a booking." });
        if (request.Status == BookingStatus.Completed && booking.StartTime > DateTime.UtcNow)
            return BadRequest(new { message = "A future booking cannot be completed." });

        booking.Status = request.Status;
        booking.AdminComment = string.IsNullOrWhiteSpace(request.AdminComment) ? null : request.AdminComment.Trim();
        await context.SaveChangesAsync();
        return Ok(booking.ToResponse());
    }

    private static IQueryable<Booking> WithDetails(IQueryable<Booking> query) =>
        query.Include(booking => booking.User)
            .Include(booking => booking.Vehicle)
            .Include(booking => booking.Service);
}
