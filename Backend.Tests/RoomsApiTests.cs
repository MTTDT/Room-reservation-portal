using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Backend.Tests;

public class RoomsApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated_WithEquipment()
    {
        var response = await _client.PostAsJsonAsync(ApiRoutes.Rooms, new
        {
            name = "Aurora Test",
            capacity = 8,
            floor = 2,
            location = "East wing",
            isActive = true,
            equipment = new[] { "Projector", "Whiteboard" }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var room = await response.Content.ReadFromJsonAsync<RoomDto>();
        Assert.NotNull(room);
        Assert.Equal(8, room.Capacity);
        Assert.True(room.IsActive);
        Assert.Contains("Projector", room.Equipment);
        Assert.Contains("Whiteboard", room.Equipment);
    }

    [Fact]
    public async Task Create_WithoutEquipment_ReturnsCreated_WithEmptyEquipmentList()
    {
        var response = await _client.PostAsJsonAsync(ApiRoutes.Rooms, new
        {
            name = "Empty Room",
            capacity = 4,
            floor = 0,
            location = "Ground floor"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var room = await response.Content.ReadFromJsonAsync<RoomDto>();
        Assert.Empty(room!.Equipment);
    }

    [Fact]
    public async Task Create_ZeroCapacity_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(ApiRoutes.Rooms, new
        {
            name = "Broken Room",
            capacity = 0,
            floor = 1,
            location = "Nowhere"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_UnknownEquipment_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(ApiRoutes.Rooms, new
        {
            name = "Coffee Room",
            capacity = 4,
            floor = 1,
            location = "Kitchen",
            equipment = new[] { "CoffeeMachine" }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_AndContainsCreatedRoom()
    {
        var id = await _client.CreateRoomAsync();

        var response = await _client.GetAsync(ApiRoutes.Rooms);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rooms = await response.Content.ReadFromJsonAsync<List<RoomDto>>();
        Assert.Contains(rooms!, r => r.Id == id);
    }

    [Fact]
    public async Task GetById_ExistingRoom_ReturnsOk()
    {
        var id = await _client.CreateRoomAsync(isActive: false);

        var room = await _client.GetFromJsonAsync<RoomDto>($"{ApiRoutes.Rooms}/{id}");

        Assert.NotNull(room);
        Assert.False(room.IsActive);
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"{ApiRoutes.Rooms}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}