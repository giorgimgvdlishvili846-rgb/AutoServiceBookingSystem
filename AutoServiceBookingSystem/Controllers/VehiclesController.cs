using System.Security.Claims;
using AutoServiceBookingSystem.Data;
using AutoServiceBookingSystem.DTOs;
using AutoServiceBookingSystem.Mappings;
using AutoServiceBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBookingSystem.Controllers;

[ApiController]
[Authorize(Roles = "Customer")]
[Route("api/vehicles")]
public sealed class VehiclesController(ApplicationDbContext context) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VehicleResponse>>> GetAll()
    {
        var vehicles = await context.Vehicles
            .Where(vehicle => vehicle.UserId == CurrentUserId)
            .OrderBy(vehicle => vehicle.Make)
            .ThenBy(vehicle => vehicle.Model)
            .ToListAsync();
        return Ok(vehicles.Select(vehicle => vehicle.ToResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<VehicleResponse>> Create(VehicleRequest request)
    {
        var plate = request.LicensePlate.Trim().ToUpperInvariant();
        if (await context.Vehicles.AnyAsync(vehicle => vehicle.LicensePlate == plate))
            return Conflict(new { message = "License plate is already registered." });

        var vehicle = new Vehicle
        {
            Make = request.Make.Trim(),
            Model = request.Model.Trim(),
            Year = request.Year,
            LicensePlate = plate,
            UserId = CurrentUserId
        };

        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), vehicle.ToResponse());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VehicleResponse>> Update(int id, VehicleRequest request)
    {
        var vehicle = await context.Vehicles
            .SingleOrDefaultAsync(vehicle => vehicle.Id == id && vehicle.UserId == CurrentUserId);
        if (vehicle is null)
            return NotFound();

        var plate = request.LicensePlate.Trim().ToUpperInvariant();
        if (await context.Vehicles.AnyAsync(other => other.Id != id && other.LicensePlate == plate))
            return Conflict(new { message = "License plate is already registered." });

        vehicle.Make = request.Make.Trim();
        vehicle.Model = request.Model.Trim();
        vehicle.Year = request.Year;
        vehicle.LicensePlate = plate;
        await context.SaveChangesAsync();
        return Ok(vehicle.ToResponse());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var vehicle = await context.Vehicles
            .Include(vehicle => vehicle.Bookings)
            .SingleOrDefaultAsync(vehicle => vehicle.Id == id && vehicle.UserId == CurrentUserId);
        if (vehicle is null)
            return NotFound();
        if (vehicle.Bookings.Count > 0)
            return Conflict(new { message = "A vehicle with booking history cannot be deleted." });

        context.Vehicles.Remove(vehicle);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
