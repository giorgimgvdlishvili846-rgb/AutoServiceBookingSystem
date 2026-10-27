using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace AutoServiceBookingSystem.Tests;

public sealed class ApiIntegrationTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Register_And_Profile_Work_With_Jwt()
    {
        using var client = factory.CreateClient();
        var auth = await RegisterAsync(client);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        var profile = await client.GetAsync("/api/auth/me");

        Assert.True(profile.StatusCode == HttpStatusCode.OK,
            $"Expected 200 but received {(int)profile.StatusCode}. Headers: {profile.Headers}");
        var json = await profile.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal(auth.Username, json!["username"]!.GetValue<string>());
        Assert.Equal("Customer", json["role"]!.GetValue<string>());
    }

    [Fact]
    public async Task Booking_Rejects_Overlapping_Active_Time()
    {
        using var client = factory.CreateClient();
        var auth = await RegisterAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var vehicleId = await AddVehicleAsync(client);
        var serviceId = await GetFirstServiceIdAsync(client);
        var startTime = NextWorkingTimeUtc();
        var request = new { vehicleId, serviceId, startTime, customerNote = "Integration test" };

        var first = await client.PostAsJsonAsync("/api/bookings", request);
        var second = await client.PostAsJsonAsync("/api/bookings", request);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Admin_Can_Confirm_But_Cannot_Reject_Confirmed_Booking()
    {
        using var customerClient = factory.CreateClient();
        var customer = await RegisterAsync(customerClient);
        customerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customer.Token);
        var vehicleId = await AddVehicleAsync(customerClient);
        var serviceId = await GetFirstServiceIdAsync(customerClient);
        var create = await customerClient.PostAsJsonAsync("/api/bookings", new
        {
            vehicleId,
            serviceId,
            startTime = NextWorkingTimeUtc().AddDays(7),
            customerNote = "Status test"
        });
        var booking = await create.Content.ReadFromJsonAsync<JsonObject>();
        var bookingId = booking!["id"]!.GetValue<int>();

        using var adminClient = factory.CreateClient();
        var adminToken = await LoginAsync(adminClient, "admin", "Admin123!");
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var confirmed = await adminClient.PutAsJsonAsync($"/api/bookings/admin/{bookingId}/status",
            new { status = "Confirmed", adminComment = "Approved" });
        var rejected = await adminClient.PutAsJsonAsync($"/api/bookings/admin/{bookingId}/status",
            new { status = "Rejected", adminComment = "Too late" });

        Assert.Equal(HttpStatusCode.OK, confirmed.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, rejected.StatusCode);
    }

    [Fact]
    public async Task Blocked_Customer_Cannot_Create_Booking()
    {
        using var customerClient = factory.CreateClient();
        var customer = await RegisterAsync(customerClient);
        customerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customer.Token);
        var vehicleId = await AddVehicleAsync(customerClient);
        var serviceId = await GetFirstServiceIdAsync(customerClient);

        using var adminClient = factory.CreateClient();
        var adminToken = await LoginAsync(adminClient, "admin", "Admin123!");
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var blocked = await adminClient.PutAsJsonAsync($"/api/admin/users/{customer.UserId}/block",
            new { isBlocked = true });

        var response = await customerClient.PostAsJsonAsync("/api/bookings", new
        {
            vehicleId,
            serviceId,
            startTime = NextWorkingTimeUtc().AddDays(14),
            customerNote = "Should fail"
        });

        Assert.Equal(HttpStatusCode.OK, blocked.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<(string Token, string Username, int UserId)> RegisterAsync(HttpClient client)
    {
        var username = $"user{Guid.NewGuid():N}"[..20];
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            email = $"{username}@test.local",
            firstName = "Test",
            lastName = "Customer",
            password = "Password123!"
        });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonObject>();
        return (
            json!["token"]!.GetValue<string>(),
            username,
            json["user"]!["id"]!.GetValue<int>());
    }

    private static async Task<string> LoginAsync(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonObject>();
        return json!["token"]!.GetValue<string>();
    }

    private static async Task<int> AddVehicleAsync(HttpClient client)
    {
        var plate = $"T{Guid.NewGuid():N}"[..10].ToUpperInvariant();
        var response = await client.PostAsJsonAsync("/api/vehicles", new
        {
            make = "Toyota",
            model = "Camry",
            year = 2024,
            licensePlate = plate
        });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonObject>();
        return json!["id"]!.GetValue<int>();
    }

    private static async Task<int> GetFirstServiceIdAsync(HttpClient client)
    {
        var services = await client.GetFromJsonAsync<JsonArray>("/api/services");
        return services![0]!["id"]!.GetValue<int>();
    }

    private static DateTime NextWorkingTimeUtc()
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tbilisi");
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).Date.AddDays(2).AddHours(10);
        while (local.DayOfWeek == DayOfWeek.Sunday)
            local = local.AddDays(1);
        return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), timeZone);
    }
}
