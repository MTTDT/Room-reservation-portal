using Backend.Exceptions;
using Backend.DTOs.Users;
using Backend.Models;
using Backend.Services;
using FluentAssertions;

namespace Backend.Tests;

public class UserServiceTests
{
    private static User CreateUser(int id = 1, string name = "John", string surname = "Doe") => new()
    {
        Id = id,
        Name = name,
        Surname = surname
    };

    private static Room CreateRoom(int id = 1) => new()
    {
        Id = id,
        Name = $"Room {id}",
        Capacity = 10,
        Floor = 1,
        Location = "Vilniaus g. 22",
        Status = RoomStatus.Active,
        Equipment = []
    };

    private static Reservation CreateReservation(int id, int userId, int roomId, DateTime start, DateTime end) => new()
    {
        Id = id,
        Title = $"Meeting {id}",
        UserId = userId,
        RoomId = roomId,
        StartTime = start,
        EndTime = end,
        AttendeeCount = 2,
        Status = ReservationStatus.Booked
    };

    private static CreateUserRequest CreateCreateRequest(string name = "Jane", string surname = "Smith") => new()
    {
        Name = name,
        Surname = surname
    };

    private static UpdateUserRequest CreateUpdateRequest(string? name = "Updated", string? surname = "Person") => new()
    {
        Name = name,
        Surname = surname
    };


    [Fact]
    public async Task GetAllUsersAsync_WhenNoUsers_ReturnsEmptyList()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new UserService(db);

        var result = await service.GetAllUsersAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsUsersOrderedById()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.AddRange(
            CreateUser(id: 3, name: "C"),
            CreateUser(id: 1, name: "A"),
            CreateUser(id: 2, name: "B"));
        await db.SaveChangesAsync();
        var service = new UserService(db);

        var result = await service.GetAllUsersAsync();

        result.Select(u => u.Id).Should().Equal(1, 2, 3);
        result.Select(u => u.Name).Should().Equal("A", "B", "C");
    }

    [Fact]
    public async Task GetAllUsersAsync_CountsOnlyActiveRegistrationsOfEachUser()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.AddRange(CreateUser(id: 1), CreateUser(id: 2, name: "Other"));
        db.Rooms.Add(CreateRoom());
        db.Reservations.AddRange(
            CreateReservation(1, userId: 1, roomId: 1, DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(1)),
            CreateReservation(2, userId: 1, roomId: 1, DateTime.Now.AddDays(2), DateTime.Now.AddDays(2).AddHours(1)),
            CreateReservation(3, userId: 1, roomId: 1, DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-2).AddHours(1)),
            CreateReservation(4, userId: 2, roomId: 1, DateTime.Now.AddDays(3), DateTime.Now.AddDays(3).AddHours(1)));
        await db.SaveChangesAsync();
        var service = new UserService(db);

        var result = await service.GetAllUsersAsync();

        result.Single(u => u.Id == 1).ActiveRegistrations.Should().Be(2);
        result.Single(u => u.Id == 2).ActiveRegistrations.Should().Be(1);
    }

    [Fact]
    public async Task GetAllUsersAsync_WhenUserHasNoReservations_ReturnsZeroActiveRegistrations()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(CreateUser());
        await db.SaveChangesAsync();
        var service = new UserService(db);

        var result = await service.GetAllUsersAsync();

        result.Should().ContainSingle()
            .Which.ActiveRegistrations.Should().Be(0);
    }


    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ReturnsMappedUser()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(CreateUser());
        await db.SaveChangesAsync();
        var service = new UserService(db);

        var result = await service.GetUserByIdAsync(1);

        result.Id.Should().Be(1);
        result.Name.Should().Be("John");
        result.Surname.Should().Be("Doe");
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new UserService(db);

        var act = () => service.GetUserByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }


    [Fact]
    public async Task CreateUserAsync_PersistsUserAndReturnsResponse()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new UserService(db);

        var result = await service.CreateUserAsync(CreateCreateRequest());

        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Jane");
        result.Surname.Should().Be("Smith");

        var stored = await db.Users.FindAsync(result.Id);
        stored.Should().NotBeNull();
        stored.Name.Should().Be("Jane");
        stored.Surname.Should().Be("Smith");
    }

    [Fact]
    public async Task CreateUserAsync_TrimsNameAndSurname()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new UserService(db);

        var result = await service.CreateUserAsync(CreateCreateRequest(name: "  Jane  ", surname: "  Smith  "));

        result.Name.Should().Be("Jane");
        result.Surname.Should().Be("Smith");

        var stored = await db.Users.FindAsync(result.Id);
        stored!.Name.Should().Be("Jane");
        stored.Surname.Should().Be("Smith");
    }


    [Fact]
    public async Task UpdateUserAsync_WhenUserExists_UpdatesNameAndSurname()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(CreateUser());
        await db.SaveChangesAsync();
        var service = new UserService(db);

        var result = await service.UpdateUserAsync(1, CreateUpdateRequest());

        result.Id.Should().Be(1);
        result.Name.Should().Be("Updated");
        result.Surname.Should().Be("Person");

        var stored = await db.Users.FindAsync(1);
        stored!.Name.Should().Be("Updated");
        stored.Surname.Should().Be("Person");
    }

    [Fact]
    public async Task UpdateUserAsync_TrimsNameAndSurname()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(CreateUser());
        await db.SaveChangesAsync();
        var service = new UserService(db);

        var result = await service.UpdateUserAsync(1, CreateUpdateRequest(name: "  Trimmed  ", surname: "  Name  "));

        result.Name.Should().Be("Trimmed");
        result.Surname.Should().Be("Name");
    }

    [Fact]
    public async Task UpdateUserAsync_WhenNameIsNull_KeepsExistingName()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(CreateUser());
        await db.SaveChangesAsync();
        var service = new UserService(db);

        var result = await service.UpdateUserAsync(1, CreateUpdateRequest(name: null, surname: "Changed"));

        result.Name.Should().Be("John");
        result.Surname.Should().Be("Changed");
    }

    [Fact]
    public async Task UpdateUserAsync_WhenSurnameIsNull_KeepsExistingSurname()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(CreateUser());
        await db.SaveChangesAsync();
        var service = new UserService(db);

        var result = await service.UpdateUserAsync(1, CreateUpdateRequest(name: "Changed", surname: null));

        result.Name.Should().Be("Changed");
        result.Surname.Should().Be("Doe");
    }

    [Fact]
    public async Task UpdateUserAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new UserService(db);

        var act = () => service.UpdateUserAsync(999, CreateUpdateRequest());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateUserAsync_DoesNotAffectOtherUsers()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.AddRange(CreateUser(id: 1, name: "User A"), CreateUser(id: 2, name: "User B"));
        await db.SaveChangesAsync();
        var service = new UserService(db);

        await service.UpdateUserAsync(1, CreateUpdateRequest(name: "Changed"));

        var other = await db.Users.FindAsync(2);
        other!.Name.Should().Be("User B");
    }


    [Fact]
    public async Task DeleteUserAsync_WhenUserExists_RemovesIt()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(CreateUser());
        await db.SaveChangesAsync();
        var service = new UserService(db);

        await service.DeleteUserAsync(1);

        db.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserDoesNotExist_ThrowsNotFoundExceptionAndKeepsOtherUsers()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.Add(CreateUser());
        await db.SaveChangesAsync();
        var service = new UserService(db);

        var act = () => service.DeleteUserAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
        db.Users.Should().ContainSingle();
    }

    [Fact]
    public async Task DeleteUserAsync_OnlyRemovesTargetedUser()
    {
        await using var db = TestDbContextFactory.Create();
        db.Users.AddRange(CreateUser(id: 1), CreateUser(id: 2, name: "User B"));
        await db.SaveChangesAsync();
        var service = new UserService(db);

        await service.DeleteUserAsync(1);

        db.Users.Select(u => u.Id).Should().Equal(2);
    }
}