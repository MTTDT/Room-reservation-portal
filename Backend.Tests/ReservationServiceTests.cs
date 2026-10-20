using System.ComponentModel.DataAnnotations;
using Backend.Exceptions;
using Backend.DTOs.Reservations;
using Backend.Models;
using Backend.Data;
using Backend.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Backend.AppOptions;

namespace Backend.Tests;

public class ReservationServiceTests
{
    private const int MaxDurationDays = 2;
    private const int MaxAdvanceDays = 30;

    private static readonly DateTime BaseStart = DateTime.UtcNow.Date.AddDays(2).AddHours(10);

    private static ReservationService CreateService(AppDbContext db) =>
        new(db, Options.Create(new ReservationOptions
        {
            MaxDurationDays = MaxDurationDays,
            MaxAdvanceReservationDays = MaxAdvanceDays
        }));

    private static void SeedBaseData(AppDbContext db)
    {
        db.Users.AddRange(
            new User { Id = 1, Name = "John", Surname = "Doe" },
            new User { Id = 2, Name = "Jane", Surname = "Smith" });

        db.Rooms.AddRange(
            CreateRoom(id: 1, name: "Room A"),
            CreateRoom(id: 2, name: "Room B"));

        db.SaveChanges();
    }

    private static Room CreateRoom(
        int id,
        string name,
        int capacity = 10,
        RoomStatus status = RoomStatus.Active) => new()
    {
        Id = id,
        Name = name,
        Capacity = capacity,
        Floor = 1,
        Location = "Vilniaus g. 22",
        Status = status,
        Equipment = []
    };

    private static Reservation CreateReservation(
        int id = 1,
        int userId = 1,
        int roomId = 1,
        DateTime? start = null,
        DateTime? end = null,
        int attendeeCount = 5,
        ReservationStatus status = ReservationStatus.Booked)
    {
        var s = start ?? BaseStart;
        return new Reservation
        {
            Id = id,
            Title = $"Meeting {id}",
            UserId = userId,
            RoomId = roomId,
            StartTime = s,
            EndTime = end ?? s.AddHours(1),
            AttendeeCount = attendeeCount,
            Status = status
        };
    }

    private static CreateReservationRequest CreateCreateRequest(
        int userId = 1,
        int roomId = 1,
        DateTime? start = null,
        DateTime? end = null,
        int attendeeCount = 5,
        string title = "New meeting")
    {
        var s = start ?? BaseStart;
        return new CreateReservationRequest
        {
            Title = title,
            UserId = userId,
            RoomId = roomId,
            StartTime = s,
            EndTime = end ?? s.AddHours(1),
            AttendeeCount = attendeeCount
        };
    }

    private static UpdateReservationRequest CreateUpdateRequest(
        DateTime? start = null,
        DateTime? end = null,
        int attendeeCount = 6,
        string title = "Updated meeting")
    {
        var s = start ?? BaseStart.AddHours(2);
        return new UpdateReservationRequest
        {
            Title = title,
            StartTime = s,
            EndTime = end ?? s.AddHours(1),
            AttendeeCount = attendeeCount
        };
    }


    [Fact]
    public async Task GetAllReservationsAsync_WhenNoReservations_ReturnsEmptyList()
    {
        await using var db = TestDbContextFactory.Create();
        var service = CreateService(db);

        var result = await service.GetAllReservationsAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllReservationsAsync_ReturnsReservationsOrderedById()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.AddRange(
            CreateReservation(id: 3, roomId: 2, start: BaseStart),
            CreateReservation(id: 1, roomId: 1, start: BaseStart),
            CreateReservation(id: 2, roomId: 1, start: BaseStart.AddHours(3)));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.GetAllReservationsAsync();

        result.Select(r => r.Id).Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task GetAllReservationsAsync_FilterByRoomName_ReturnsOnlyMatching()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.AddRange(
            CreateReservation(id: 1, roomId: 1, start: BaseStart),
            CreateReservation(id: 2, roomId: 2, start: BaseStart.AddHours(3)));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.GetAllReservationsAsync(
            new FilterReservationRequest { RoomName = "oom a" });

        result.Select(r => r.Id).Should().Equal(1);
    }



    [Fact]
    public async Task GetReservationByIdAsync_WhenReservationExists_ReturnsMappedReservation()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.Add(CreateReservation(id: 1, userId: 1, roomId: 1, attendeeCount: 4));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.GetReservationByIdAsync(1);

        result.Id.Should().Be(1);
        result.Title.Should().Be("Meeting 1");
        result.userId.Should().Be(1);
        result.UserName.Should().Be("John");
        result.UserSurname.Should().Be("Doe");
        result.RoomId.Should().Be(1);
        result.RoomName.Should().Be("Room A");
        result.StartTime.Should().Be(BaseStart);
        result.EndTime.Should().Be(BaseStart.AddHours(1));
        result.AttendeeCount.Should().Be(4);
        result.Status.Should().Be(ReservationStatus.Booked);
    }

    [Fact]
    public async Task GetReservationByIdAsync_WhenReservationDoesNotExist_ThrowsNotFoundException()
    {
        await using var db = TestDbContextFactory.Create();
        var service = CreateService(db);

        var act = () => service.GetReservationByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }



    [Fact]
    public async Task CreateReservationAsync_WithValidRequest_PersistsAndReturnsResponse()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        var service = CreateService(db);

        var result = await service.CreateReservationAsync(CreateCreateRequest(attendeeCount: 7, title: "Planning"));

        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be("Planning");
        result.userId.Should().Be(1);
        result.UserName.Should().Be("John");
        result.RoomId.Should().Be(1);
        result.RoomName.Should().Be("Room A");
        result.StartTime.Should().Be(BaseStart);
        result.EndTime.Should().Be(BaseStart.AddHours(1));
        result.AttendeeCount.Should().Be(7);
        result.Status.Should().Be(ReservationStatus.Booked);

        db.Reservations.Should().ContainSingle();
    }

    [Fact]
    public async Task CreateReservationAsync_WhenAttendeeCountEqualsCapacity_Succeeds()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        var service = CreateService(db);

        var result = await service.CreateReservationAsync(CreateCreateRequest(attendeeCount: 10));

        result.AttendeeCount.Should().Be(10);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenDurationEqualsMaximum_Succeeds()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        var service = CreateService(db);
        var request = CreateCreateRequest(end: BaseStart.AddDays(MaxDurationDays));

        var act = () => service.CreateReservationAsync(request);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreateReservationAsync_WhenStartIsWithinAdvanceLimit_Succeeds()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        var service = CreateService(db);
        var start = DateTime.UtcNow.AddDays(MaxAdvanceDays - 1);

        var act = () => service.CreateReservationAsync(CreateCreateRequest(start: start));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreateReservationAsync_BackToBackInSameRoom_Succeeds()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.Add(CreateReservation(id: 1, userId: 1, roomId: 1, start: BaseStart, end: BaseStart.AddHours(1)));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var request = CreateCreateRequest(userId: 2, roomId: 1, start: BaseStart.AddHours(1));

        var act = () => service.CreateReservationAsync(request);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreateReservationAsync_SameTimeInDifferentRoomByDifferentUser_Succeeds()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.Add(CreateReservation(id: 1, userId: 1, roomId: 1));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var act = () => service.CreateReservationAsync(CreateCreateRequest(userId: 2, roomId: 2));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreateReservationAsync_WhenOverlappingReservationIsNotBooked_Succeeds()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.Add(CreateReservation(id: 1, userId: 1, roomId: 1, status: ReservationStatus.Cancelled));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var act = () => service.CreateReservationAsync(CreateCreateRequest(userId: 1, roomId: 1));

        await act.Should().NotThrowAsync();
    }


    [Fact]
    public async Task CreateReservationAsync_WhenStartIsInThePast_ThrowsAppValidationException()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        var service = CreateService(db);
        var request = CreateCreateRequest(start: DateTime.UtcNow.AddHours(-1), end: DateTime.UtcNow.AddHours(1));

        var act = () => service.CreateReservationAsync(request);

        await act.Should().ThrowAsync<AppValidationException>();
        db.Reservations.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateReservationAsync_WhenEndIsBeforeStart_ThrowsAppValidationException()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        var service = CreateService(db);
        var request = CreateCreateRequest(start: BaseStart, end: BaseStart.AddHours(-1));

        var act = () => service.CreateReservationAsync(request);

        await act.Should().ThrowAsync<AppValidationException>();
    }

    [Fact]
    public async Task CreateReservationAsync_WhenEndEqualsStart_ThrowsAppValidationException()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        var service = CreateService(db);
        var request = CreateCreateRequest(start: BaseStart, end: BaseStart);

        var act = () => service.CreateReservationAsync(request);

        await act.Should().ThrowAsync<AppValidationException>();
    }

    [Fact]
    public async Task CreateReservationAsync_WhenDurationExceedsMaximum_ThrowsAppValidationException()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        var service = CreateService(db);
        var request = CreateCreateRequest(end: BaseStart.AddDays(MaxDurationDays).AddHours(1));

        var act = () => service.CreateReservationAsync(request);

        await act.Should().ThrowAsync<AppValidationException>();
    }

    [Fact]
    public async Task CreateReservationAsync_WhenStartIsBeyondAdvanceLimit_ThrowsAppValidationException()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        var service = CreateService(db);
        var start = DateTime.UtcNow.AddDays(MaxAdvanceDays + 1);

        var act = () => service.CreateReservationAsync(CreateCreateRequest(start: start));

        await act.Should().ThrowAsync<AppValidationException>();
    }

    [Fact]
    public async Task CreateReservationAsync_WhenRoomDoesNotExist_ThrowsAppValidationException()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        var service = CreateService(db);

        var act = () => service.CreateReservationAsync(CreateCreateRequest(roomId: 999));

        await act.Should().ThrowAsync<AppValidationException>();
    }

    [Fact]
    public async Task CreateReservationAsync_WhenRoomIsInactive_ThrowsAppValidationException()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Rooms.Add(CreateRoom(id: 3, name: "Closed room", status: RoomStatus.Inactive));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var act = () => service.CreateReservationAsync(CreateCreateRequest(roomId: 3));

        await act.Should().ThrowAsync<AppValidationException>();
    }

    [Fact]
    public async Task CreateReservationAsync_WhenAttendeeCountExceedsCapacity_ThrowsAppValidationException()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        var service = CreateService(db);

        var act = () => service.CreateReservationAsync(CreateCreateRequest(attendeeCount: 11));

        await act.Should().ThrowAsync<AppValidationException>();
    }

    [Fact]
    public async Task CreateReservationAsync_WhenRoomIsAlreadyBooked_ThrowsAppValidationException()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.Add(CreateReservation(id: 1, userId: 1, roomId: 1, start: BaseStart, end: BaseStart.AddHours(2)));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var request = CreateCreateRequest(userId: 2, roomId: 1, start: BaseStart.AddHours(1), end: BaseStart.AddHours(3));

        var act = () => service.CreateReservationAsync(request);

        await act.Should().ThrowAsync<AppValidationException>();
        db.Reservations.Should().ContainSingle();
    }

    [Fact]
    public async Task CreateReservationAsync_WhenNewReservationFullyContainsExistingOne_ThrowsAppValidationException()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.Add(CreateReservation(id: 1, userId: 1, roomId: 1, start: BaseStart.AddHours(1), end: BaseStart.AddHours(2)));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var request = CreateCreateRequest(userId: 2, roomId: 1, start: BaseStart, end: BaseStart.AddHours(4));

        var act = () => service.CreateReservationAsync(request);

        await act.Should().ThrowAsync<AppValidationException>();
    }

    [Fact]
    public async Task CreateReservationAsync_WhenUserAlreadyHasReservationAtThatTime_ThrowsAppValidationException()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.Add(CreateReservation(id: 1, userId: 1, roomId: 1));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var act = () => service.CreateReservationAsync(CreateCreateRequest(userId: 1, roomId: 2));

        await act.Should().ThrowAsync<AppValidationException>().WithMessage("*already have a reservation*");
    }


    [Fact]
    public async Task UpdateReservationAsync_WithValidRequest_UpdatesFieldsAndKeepsRoomAndUser()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.Add(CreateReservation(id: 1, userId: 1, roomId: 1));
        await db.SaveChangesAsync();
        var service = CreateService(db);
        var request = CreateUpdateRequest(start: BaseStart.AddHours(5), attendeeCount: 8, title: "Renamed");

        var result = await service.UpdateReservationAsync(1, request);

        result.Id.Should().Be(1);
        result.Title.Should().Be("Renamed");
        result.StartTime.Should().Be(BaseStart.AddHours(5));
        result.EndTime.Should().Be(BaseStart.AddHours(6));
        result.AttendeeCount.Should().Be(8);
        result.RoomId.Should().Be(1);
        result.userId.Should().Be(1);

        var stored = await db.Reservations.FindAsync(1);
        stored!.Title.Should().Be("Renamed");
        stored.StartTime.Should().Be(BaseStart.AddHours(5));
    }

    [Fact]
    public async Task UpdateReservationAsync_WhenNewTimeOverlapsItself_Succeeds()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.Add(CreateReservation(id: 1, start: BaseStart, end: BaseStart.AddHours(2)));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var request = CreateUpdateRequest(start: BaseStart, end: BaseStart.AddHours(3));

        var act = () => service.UpdateReservationAsync(1, request);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateReservationAsync_WhenReservationDoesNotExist_ThrowsNotFoundException()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        var service = CreateService(db);

        var act = () => service.UpdateReservationAsync(999, CreateUpdateRequest());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateReservationAsync_WhenNewTimeOverlapsAnotherReservationInSameRoom_ThrowsAppValidationException()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.AddRange(
            CreateReservation(id: 1, userId: 1, roomId: 1, start: BaseStart, end: BaseStart.AddHours(1)),
            CreateReservation(id: 2, userId: 2, roomId: 1, start: BaseStart.AddHours(3), end: BaseStart.AddHours(4)));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var request = CreateUpdateRequest(start: BaseStart.AddHours(3), end: BaseStart.AddHours(4));

        var act = () => service.UpdateReservationAsync(1, request);

        await act.Should().ThrowAsync<AppValidationException>();
    }

    [Fact]
    public async Task UpdateReservationAsync_WhenNewTimeOverlapsAnotherReservationOfSameUser_ThrowsAppValidationException()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.AddRange(
            CreateReservation(id: 1, userId: 1, roomId: 1, start: BaseStart, end: BaseStart.AddHours(1)),
            CreateReservation(id: 2, userId: 1, roomId: 2, start: BaseStart.AddHours(3), end: BaseStart.AddHours(4)));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var request = CreateUpdateRequest(start: BaseStart.AddHours(3), end: BaseStart.AddHours(4));

        var act = () => service.UpdateReservationAsync(1, request);

        await act.Should().ThrowAsync<AppValidationException>().WithMessage("*already have a reservation*");
    }

    [Fact]
    public async Task UpdateReservationAsync_WhenAttendeeCountExceedsCapacity_ThrowsAppValidationException()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.Add(CreateReservation(id: 1));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var act = () => service.UpdateReservationAsync(1, CreateUpdateRequest(attendeeCount: 11));

        await act.Should().ThrowAsync<AppValidationException>().WithMessage("*exceeds the room capacity*");
    }

    [Fact]
    public async Task UpdateReservationAsync_WhenEndIsBeforeStart_ThrowsAppValidationException()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.Add(CreateReservation(id: 1));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var request = CreateUpdateRequest(start: BaseStart.AddHours(5), end: BaseStart.AddHours(4));

        var act = () => service.UpdateReservationAsync(1, request);

        await act.Should().ThrowAsync<AppValidationException>();
    }

    [Fact]
    public async Task UpdateReservationAsync_WhenValidationFails_DoesNotChangeStoredReservation()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.Add(CreateReservation(id: 1, attendeeCount: 5));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var act = () => service.UpdateReservationAsync(1, CreateUpdateRequest(attendeeCount: 11, title: "Should not persist"));
        await act.Should().ThrowAsync<AppValidationException>();

        var stored = await db.Reservations.FindAsync(1);
        stored!.Title.Should().Be("Meeting 1");
        stored.AttendeeCount.Should().Be(5);
    }

    [Fact]
    public async Task UpdateReservationAsync_DoesNotAffectOtherReservations()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.AddRange(
            CreateReservation(id: 1, userId: 1, roomId: 1),
            CreateReservation(id: 2, userId: 2, roomId: 2));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        await service.UpdateReservationAsync(1, CreateUpdateRequest(title: "Changed"));

        var other = await db.Reservations.FindAsync(2);
        other!.Title.Should().Be("Meeting 2");
    }


    [Fact]
    public async Task DeleteReservationAsync_WhenReservationExists_RemovesIt()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.Add(CreateReservation(id: 1));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        await service.DeleteReservationAsync(1);

        db.Reservations.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteReservationAsync_WhenReservationDoesNotExist_ThrowsNotFoundExceptionAndKeepsOthers()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.Add(CreateReservation(id: 1));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var act = () => service.DeleteReservationAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
        db.Reservations.Should().ContainSingle();
    }

    [Fact]
    public async Task DeleteReservationAsync_OnlyRemovesTargetedReservation()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.AddRange(
            CreateReservation(id: 1, userId: 1, roomId: 1),
            CreateReservation(id: 2, userId: 2, roomId: 2));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        await service.DeleteReservationAsync(1);

        db.Reservations.Select(r => r.Id).Should().Equal(2);
    }

    [Fact]
    public async Task DeleteReservationAsync_FreesTheSlotForNewBookings()
    {
        await using var db = TestDbContextFactory.Create();
        SeedBaseData(db);
        db.Reservations.Add(CreateReservation(id: 1, userId: 1, roomId: 1));
        await db.SaveChangesAsync();
        var service = CreateService(db);

        await service.DeleteReservationAsync(1);
        var act = () => service.CreateReservationAsync(CreateCreateRequest(userId: 2, roomId: 1));

        await act.Should().NotThrowAsync();
    }
}