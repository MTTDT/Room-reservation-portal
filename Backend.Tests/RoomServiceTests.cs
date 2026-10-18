using Backend.DTOs.Rooms;
using Backend.Models;
using Backend.Services;
using FluentAssertions;
using Backend.Exceptions;
namespace Backend.Tests;

public class RoomServiceTests
{
    private static Room CreateRoom(int id = 1, string name = "Room A", int capacity = 10, int floor = 2, string location = "Vilniaus g. 22", RoomStatus status = RoomStatus.Active, List<RoomEquipment>? equipment = null) => new()
    {
        Id = id,
        Name = name,
        Capacity = capacity,
        Floor = floor,
        Location = location,
        Status = status,
        Equipment = equipment ?? [RoomEquipment.Projector, RoomEquipment.VideoConferencing]
    };

    private static CreateRoomRequest CreateCreateRequest(string name = "New Room", int capacity = 8, int floor = 1, string location = "Gedimino pr. 1", RoomStatus status = RoomStatus.Active, IEnumerable<RoomEquipment>? equipment = null) => new()
    {
        Name = name,
        Capacity = capacity,
        Floor = floor,
        Location = location,
        Status = status,
        Equipment = equipment == null ? [] : equipment.ToList()
    };

    private static UpdateRoomRequest CreateUpdateRequest(string name = "Updated Room", int capacity = 20, int floor = 3, string location = "Konstitucijos pr. 5", RoomStatus status = RoomStatus.Active, IEnumerable<RoomEquipment>? equipment = null) => new()
    {
        Name = name,
        Capacity = capacity,
        Floor = floor,
        Location = location,
        Status = status,
        Equipment = equipment == null ? [] : equipment.ToList()
    };


    [Fact]
    public async Task GetAllRoomsAsync_WhenNoRooms_ReturnsEmptyList()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new RoomService(db);

        var result = await service.GetAllRoomsAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllRoomsAsync_ReturnsRoomsOrderedById()
    {
        await using var db = TestDbContextFactory.Create();
        db.Rooms.AddRange(
            CreateRoom(id: 3, name: "C"),
            CreateRoom(id: 1, name: "A"),
            CreateRoom(id: 2, name: "B"));
        await db.SaveChangesAsync();
        var service = new RoomService(db);

        var result = await service.GetAllRoomsAsync();

        result.Select(r => r.Id).Should().Equal(1, 2, 3);
        result.Select(r => r.Name).Should().Equal("A", "B", "C");
    }

    [Fact]
    public async Task GetAllRoomsAsync_MapsEquipmentToStrings()
    {
        await using var db = TestDbContextFactory.Create();
        db.Rooms.Add(CreateRoom(equipment: [RoomEquipment.Projector, RoomEquipment.VideoConferencing]));
        await db.SaveChangesAsync();
        var service = new RoomService(db);

        var result = await service.GetAllRoomsAsync();

        result.Should().ContainSingle()
            .Which.Equipment.Should().BeEquivalentTo(
                nameof(RoomEquipment.Projector),
                nameof(RoomEquipment.VideoConferencing));
    }


    [Fact]
    public async Task GetRoomByIdAsync_WhenRoomExists_ReturnsMappedRoom()
    {
        await using var db = TestDbContextFactory.Create();
        db.Rooms.Add(CreateRoom());
        await db.SaveChangesAsync();
        var service = new RoomService(db);

        var result = await service.GetRoomByIdAsync(1);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Room A");
        result.Capacity.Should().Be(10);
        result.Floor.Should().Be(2);
        result.Location.Should().Be("Vilniaus g. 22");
        result.Status.Should().Be(RoomStatus.Active);
        result.Equipment.Should().BeEquivalentTo(
            nameof(RoomEquipment.Projector),
            nameof(RoomEquipment.VideoConferencing));
    }

    [Fact]
    public async Task GetRoomByIdAsync_WhenRoomDoesNotExist_ThrowsNotFoundException()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new RoomService(db);

        var act = () => service.GetRoomByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }


    [Fact]
    public async Task CreateRoomAsync_PersistsRoomAndReturnsResponse()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new RoomService(db);
        var request = CreateCreateRequest(equipment: [RoomEquipment.Projector]);

        var result = await service.CreateRoomAsync(request);

        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Room");
        result.Capacity.Should().Be(8);
        result.Floor.Should().Be(1);
        result.Location.Should().Be("Gedimino pr. 1");
        result.Equipment.Should().Equal(nameof(RoomEquipment.Projector));

        var stored = await db.Rooms.FindAsync(result.Id);
        stored.Should().NotBeNull();
        stored!.Name.Should().Be("New Room");
    }

    [Fact]
    public async Task CreateRoomAsync_TrimsNameAndLocation()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new RoomService(db);
        var request = CreateCreateRequest(name: "  Padded Room  ", location: "  Some St. 1  ");

        var result = await service.CreateRoomAsync(request);

        result.Name.Should().Be("Padded Room");
        result.Location.Should().Be("Some St. 1");
    }

    [Fact]
    public async Task CreateRoomAsync_WhenEquipmentIsNull_StoresEmptyEquipment()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new RoomService(db);
        var request = CreateCreateRequest(equipment: null);

        var result = await service.CreateRoomAsync(request);

        result.Equipment.Should().BeEmpty();
    }


    [Fact]
    public async Task UpdateRoomAsync_WhenRoomExists_UpdatesAllFields()
    {
        await using var db = TestDbContextFactory.Create();
        db.Rooms.Add(CreateRoom());
        await db.SaveChangesAsync();
        var service = new RoomService(db);
        var request = CreateUpdateRequest(
            status: RoomStatus.Active,
            equipment: [RoomEquipment.Projector]);

        var result = await service.UpdateRoomAsync(1, request);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Updated Room");
        result.Capacity.Should().Be(20);
        result.Floor.Should().Be(3);
        result.Location.Should().Be("Konstitucijos pr. 5");
        result.Equipment.Should().Equal(nameof(RoomEquipment.Projector));

        var stored = await db.Rooms.FindAsync(1);
        stored!.Name.Should().Be("Updated Room");
        stored.Equipment.Should().Equal(RoomEquipment.Projector);
    }

    [Fact]
    public async Task UpdateRoomAsync_TrimsNameAndLocation()
    {
        await using var db = TestDbContextFactory.Create();
        db.Rooms.Add(CreateRoom());
        await db.SaveChangesAsync();
        var service = new RoomService(db);

        var result = await service.UpdateRoomAsync(1, CreateUpdateRequest(name: "  Trimmed  ", location: "  Loc  "));

        result!.Name.Should().Be("Trimmed");
        result.Location.Should().Be("Loc");
    }

    [Fact]
    public async Task UpdateRoomAsync_WhenEquipmentIsNull_ClearsEquipment()
    {
        await using var db = TestDbContextFactory.Create();
        db.Rooms.Add(CreateRoom());
        await db.SaveChangesAsync();
        var service = new RoomService(db);

        var result = await service.UpdateRoomAsync(1, CreateUpdateRequest(equipment: null));

        result!.Equipment.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateRoomAsync_WhenRoomDoesNotExist_ThrowsNotFoundException()
    {
        await using var db = TestDbContextFactory.Create();
        var service = new RoomService(db);

        var act = () => service.UpdateRoomAsync(999, CreateUpdateRequest());


        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateRoomAsync_DoesNotAffectOtherRooms()
    {
        await using var db = TestDbContextFactory.Create();
        db.Rooms.AddRange(CreateRoom(id: 1, name: "Room A"), CreateRoom(id: 2, name: "Room B"));
        await db.SaveChangesAsync();
        var service = new RoomService(db);

        await service.UpdateRoomAsync(1, CreateUpdateRequest(name: "Changed"));

        var other = await db.Rooms.FindAsync(2);
        other!.Name.Should().Be("Room B");
    }


    [Fact]
    public async Task DeleteRoomAsync_WhenRoomExists_RemovesIt()
    {
        await using var db = TestDbContextFactory.Create();
        db.Rooms.Add(CreateRoom());
        await db.SaveChangesAsync();
        var service = new RoomService(db);

        await service.DeleteRoomAsync(1);

        db.Rooms.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteRoomAsync_WhenRoomDoesNotExist_ThrowsNotFoundException()
    {
        await using var db = TestDbContextFactory.Create();
        db.Rooms.Add(CreateRoom());
        await db.SaveChangesAsync();
        var service = new RoomService(db);

        var act = () => service.DeleteRoomAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
        db.Rooms.Should().ContainSingle();
    }

    [Fact]
    public async Task DeleteRoomAsync_OnlyRemovesTargetedRoom()
    {
        await using var db = TestDbContextFactory.Create();
        db.Rooms.AddRange(CreateRoom(id: 1), CreateRoom(id: 2, name: "Room B"));
        await db.SaveChangesAsync();
        var service = new RoomService(db);

        await service.DeleteRoomAsync(1);

        db.Rooms.Select(r => r.Id).Should().Equal(2);
    }
}