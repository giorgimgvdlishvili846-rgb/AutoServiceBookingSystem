using System.Text;
using System.Text.Json.Serialization;
using AutoServiceBookingSystem.Data;
using AutoServiceBookingSystem.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AutoServiceBookingSystem;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();
        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddProblemDetails();
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        builder.Services.AddOpenApi();
        builder.Services.AddCors(options =>
            options.AddPolicy("Frontend", policy =>
                policy.WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()));

        var jwtKey = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key is not configured.");
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });
        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.UseExceptionHandler();
        if (app.Environment.IsDevelopment())
            app.MapOpenApi();

        app.UseCors("Frontend");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        await using (var scope = app.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();
            await DbSeeder.SeedAsync(context, app.Configuration);
        }

        await app.RunAsync();
    }
}