using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Backend.Tests;

public class UsersApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_ReturnsOk_AndContainsCreatedUser()
    {
        var id = await _client.CreateUserAsync();

        var response = await _client.GetAsync(ApiRoutes.Users);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        Assert.Contains(users!, u => u.Id == id);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated_AndUserCanBeFetched()
    {
        var response = await _client.PostAsJsonAsync(ApiRoutes.Users, new
        {
            name = "Anna",
            surname = "Lee",
            email = "anna@example.com"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(created);
        Assert.Equal("Anna", created.Name);

        var fetched = await _client.GetAsync($"{ApiRoutes.Users}/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, fetched.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    public async Task Create_InvalidName_ReturnsBadRequest(string name)
    {
        var response = await _client.PostAsJsonAsync(ApiRoutes.Users, new
        {
            name,
            surname = "Lee",
            email = "invalid@example.com"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"{ApiRoutes.Users}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ExistingUser_ReturnsNoContent_AndPersistsChange()
    {
        var id = await _client.CreateUserAsync();

        var response = await _client.PutAsJsonAsync($"{ApiRoutes.Users}/{id}", new
        {
            name = "Updated",
            surname = "Person",
            email = "updated@example.com"
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var user = await _client.GetFromJsonAsync<UserDto>($"{ApiRoutes.Users}/{id}");
        Assert.Equal("Updated", user!.Name);
    }

    [Fact]
    public async Task Update_UnknownUser_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync($"{ApiRoutes.Users}/999999", new
        {
            name = "Updated",
            surname = "Person",
            email = "updated@example.com"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingUser_ReturnsNoContent_ThenUserIsGone()
    {
        var id = await _client.CreateUserAsync();

        var response = await _client.DeleteAsync($"{ApiRoutes.Users}/{id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var afterwards = await _client.GetAsync($"{ApiRoutes.Users}/{id}");
        Assert.Equal(HttpStatusCode.NotFound, afterwards.StatusCode);
    }

    [Fact]
    public async Task Delete_UnknownUser_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"{ApiRoutes.Users}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}