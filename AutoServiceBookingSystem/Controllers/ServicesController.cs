using AutoServiceBookingSystem.Data;
using AutoServiceBookingSystem.DTOs;
using AutoServiceBookingSystem.Mappings;
using AutoServiceBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBookingSystem.Controllers;

[ApiController]
[Route("api/services")]
public sealed class ServicesController(ApplicationDbContext context) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceResponse>>> GetActive()
    {
        var services = await context.Services
            .Where(service => service.IsActive)
            .OrderBy(service => service.Name)
            .ToListAsync();
        return Ok(services.Select(service => service.ToResponse()));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<ActionResult<IReadOnlyList<ServiceResponse>>> GetAllForAdmin()
    {
        var services = await context.Services.OrderBy(service => service.Name).ToListAsync();
        return Ok(services.Select(service => service.ToResponse()));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ServiceResponse>> Create(ServiceRequest request)
    {
        var name = request.Name.Trim();
        if (await context.Services.AnyAsync(service => service.Name == name))
            return Conflict(new { message = "Service name is already in use." });

        var service = new Service
        {
            Name = name,
            Description = request.Description.Trim(),
            Price = request.Price,
            DurationMinutes = request.DurationMinutes,
            IsActive = request.IsActive
        };
        context.Services.Add(service);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAllForAdmin), service.ToResponse());
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ServiceResponse>> Update(int id, ServiceRequest request)
    {
        var service = await context.Services.FindAsync(id);
        if (service is null)
            return NotFound();

        var name = request.Name.Trim();
        if (await context.Services.AnyAsync(other => other.Id != id && other.Name == name))
            return Conflict(new { message = "Service name is already in use." });

        service.Name = name;
        service.Description = request.Description.Trim();
        service.Price = request.Price;
        service.DurationMinutes = request.DurationMinutes;
        service.IsActive = request.IsActive;
        await context.SaveChangesAsync();
        return Ok(service.ToResponse());
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await context.Services.Include(service => service.Bookings)
            .SingleOrDefaultAsync(service => service.Id == id);
        if (service is null)
            return NotFound();
        if (service.Bookings.Count > 0)
            return Conflict(new { message = "A service with booking history cannot be deleted; deactivate it instead." });

        context.Services.Remove(service);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
