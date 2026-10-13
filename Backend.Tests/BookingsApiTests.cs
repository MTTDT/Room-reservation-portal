using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Backend.Tests;

public class BookingsApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_ValidBooking_ReturnsCreated()
    {
        var userId = await _client.CreateUserAsync();
        var roomId = await _client.CreateRoomAsync();

        var response = await _client.PostBookingAsync(userId, roomId, TestApi.At(9), TestApi.At(10));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var booking = await response.Content.ReadFromJsonAsync<BookingDto>();
        Assert.True(booking!.Id > 0);
    }

    [Fact]
    public async Task Create_OverlappingBooking_ReturnsConflict()
    {
        var userId = await _client.CreateUserAsync();
        var roomId = await _client.CreateRoomAsync();
        await _client.PostBookingAsync(userId, roomId, TestApi.At(9), TestApi.At(11));

        var response = await _client.PostBookingAsync(userId, roomId, TestApi.At(10), TestApi.At(12));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_BackToBackBookings_AreAllowed()
    {
        var userId = await _client.CreateUserAsync();
        var roomId = await _client.CreateRoomAsync();
        var first = await _client.PostBookingAsync(userId, roomId, TestApi.At(9), TestApi.At(10));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostBookingAsync(userId, roomId, TestApi.At(10), TestApi.At(11));

        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
    }

    [Fact]
    public async Task Create_SameSlotInDifferentRooms_IsAllowed()
    {
        var userId = await _client.CreateUserAsync();
        var roomA = await _client.CreateRoomAsync();
        var roomB = await _client.CreateRoomAsync();
        await _client.PostBookingAsync(userId, roomA, TestApi.At(9), TestApi.At(10));

        var response = await _client.PostBookingAsync(userId, roomB, TestApi.At(9), TestApi.At(10));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_InactiveRoom_ReturnsConflict()
    {
        var userId = await _client.CreateUserAsync();
        var roomId = await _client.CreateRoomAsync(isActive: false);

        var response = await _client.PostBookingAsync(userId, roomId, TestApi.At(9), TestApi.At(10));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_EndBeforeStart_ReturnsBadRequest()
    {
        var userId = await _client.CreateUserAsync();
        var roomId = await _client.CreateRoomAsync();

        var response = await _client.PostBookingAsync(userId, roomId, TestApi.At(11), TestApi.At(10));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_UnknownRoom_ReturnsNotFound()
    {
        var userId = await _client.CreateUserAsync();

        var response = await _client.PostBookingAsync(userId, 999999, TestApi.At(9), TestApi.At(10));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_UnknownUser_ReturnsNotFound()
    {
        var roomId = await _client.CreateRoomAsync();

        var response = await _client.PostBookingAsync(999999, roomId, TestApi.At(9), TestApi.At(10));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_AndContainsCreatedBooking()
    {
        var userId = await _client.CreateUserAsync();
        var roomId = await _client.CreateRoomAsync();
        var created = await _client.PostBookingAsync(userId, roomId, TestApi.At(9), TestApi.At(10));
        var bookingId = (await created.Content.ReadFromJsonAsync<BookingDto>())!.Id;

        var response = await _client.GetAsync(ApiRoutes.Bookings);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bookings = await response.Content.ReadFromJsonAsync<List<BookingDto>>();
        Assert.Contains(bookings!, b => b.Id == bookingId);
    }
}