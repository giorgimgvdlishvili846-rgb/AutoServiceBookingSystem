using System.Security.Claims;
using AutoServiceBookingSystem.Data;
using AutoServiceBookingSystem.DTOs;
using AutoServiceBookingSystem.Mappings;
using AutoServiceBookingSystem.Models;
using AutoServiceBookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceBookingSystem.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ApplicationDbContext context, ITokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();

        if (await context.Users.AnyAsync(user => user.Username == username))
            return Conflict(new { message = "Username is already in use." });

        if (await context.Users.AnyAsync(user => user.Email == email))
            return Conflict(new { message = "Email is already in use." });

        var user = new User
        {
            Username = username,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var token = tokenService.CreateToken(user);
        return CreatedAtAction(nameof(Me), new AuthResponse(token.Token, token.ExpiresAt, user.ToResponse()));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await context.Users.SingleOrDefaultAsync(user => user.Username == request.Username.Trim());
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid username or password." });

        var token = tokenService.CreateToken(user);
        return Ok(new AuthResponse(token.Token, token.ExpiresAt, user.ToResponse()));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await context.Users.FindAsync(id);
        return user is null ? NotFound() : Ok(user.ToResponse());
    }
}
