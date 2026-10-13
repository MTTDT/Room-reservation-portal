using System.Net.Http.Json;

namespace Backend.Tests;

public static class ApiRoutes
{
    public const string Users = "/api/user";
    public const string Rooms = "/api/room";
    public const string Bookings = "/api/booking";
}

// Only the fields the tests read; extra JSON properties are ignored.
public record UserDto(int Id, string Name);
public record RoomDto(int Id, string Name, int Capacity, bool IsActive, List<string> Equipment);
public record BookingDto(int Id);


public static class TestApi
{
    // Tomorrow at the given hour (UTC), so bookings are never in the past.
    public static DateTime At(int hour) => DateTime.UtcNow.Date.AddDays(1).AddHours(hour);

    public static async Task<int> CreateUserAsync(this HttpClient client)
    {
        var response = await client.PostAsJsonAsync(ApiRoutes.Users, new
        {
            name = "Test",
            surname = "User",
            email = $"{Guid.NewGuid():N}@example.com"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UserDto>())!.Id;
    }

    public static async Task<int> CreateRoomAsync(this HttpClient client, bool isActive = true)
    {
        var response = await client.PostAsJsonAsync(ApiRoutes.Rooms, new
        {
            name = $"Room {Guid.NewGuid():N}",
            capacity = 10,
            floor = 1,
            location = "Test wing",
            isActive,
            equipment = new[] { "Projector", "Whiteboard" }
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RoomDto>())!.Id;
    }

    public static Task<HttpResponseMessage> PostBookingAsync(
        this HttpClient client, int userId, int roomId, DateTime start, DateTime end) =>
        client.PostAsJsonAsync(ApiRoutes.Bookings, new
        {
            userId,
            roomId,
            startTime = start,
            endTime = end
        });
    
}