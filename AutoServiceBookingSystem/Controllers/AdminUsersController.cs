using AutoServiceBookingSystem.Data;
using AutoServiceBookingSystem.DTOs;
using AutoServiceBookingSystem.Mappings;
using AutoServiceBookingSystem.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBookingSystem.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/users")]
public sealed class AdminUsersController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetCustomers()
    {
        var users = await context.Users
            .Where(user => user.Role == UserRole.Customer)
            .OrderBy(user => user.FirstName)
            .ThenBy(user => user.LastName)
            .ToListAsync();
        return Ok(users.Select(user => user.ToResponse()));
    }

    [HttpPut("{id:int}/block")]
    public async Task<ActionResult<UserResponse>> SetBlocked(int id, BlockUserRequest request)
    {
        var user = await context.Users.SingleOrDefaultAsync(user => user.Id == id && user.Role == UserRole.Customer);
        if (user is null)
            return NotFound();

        user.IsBlocked = request.IsBlocked;
        await context.SaveChangesAsync();
        return Ok(user.ToResponse());
    }
}
