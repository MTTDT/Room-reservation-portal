using System.Linq.Expressions;
using Backend.Models;
using Backend.Data;
using Backend.DTOs.Reservations;
using Microsoft.EntityFrameworkCore;
using Backend.DTOs.Users;
using Backend.AppOptions;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using Backend.Exceptions;
using System.Data;

namespace Backend.Services
{
    public class ReservationService(AppDbContext db, IOptions<ReservationOptions> options) : IReservationService
    {
        private readonly ReservationOptions rules = options.Value;
        public async Task<IReadOnlyList<ReservationResponse>> GetAllReservationsAsync(FilterReservationRequest? filter = null, CancellationToken ct = default)
        {
            return await db.Reservations
                .AsNoTracking()
                .OrderBy(r => r.Id)
                .Where(r => 
                    (filter == null) ||
                    r.Room.Name.ToLower().Contains((filter.RoomName ?? "").ToLower()) &&
                    (r.User.Name + " " + r.User.Surname).ToLower().Contains((filter.Organizer ?? "").ToLower()) &&
                    (!filter.Status.HasValue || r.Status == filter.Status) &&
                    (!filter.From.HasValue || r.StartTime >= filter.From) &&
                    (!filter.To.HasValue || r.StartTime <= filter.To) 
                )
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<ReservationResponse> GetReservationByIdAsync(int id, CancellationToken ct = default)
        {
            var reservation = await db.Reservations
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);

            if(reservation == null)
            {
                throw NotFoundException.For<Reservation>(id);
            }
            return reservation;

            
        }

        public async Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request, CancellationToken ct = default)
        {
            await ValidateLocallyAsync(request.StartTime, request.EndTime, request.RoomId, request.UserId, request.AttendeeCount, ct);

            var reservation = new Reservation
            {
                Title = request.Title,
                RoomId = request.RoomId,
                UserId = request.UserId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                AttendeeCount = request.AttendeeCount
            };

            db.Reservations.Add(reservation);
            await db.SaveChangesAsync(ct);

            return await GetReservationByIdAsync(reservation.Id, ct);
        }

        public async Task<ReservationResponse> UpdateReservationAsync(int id, UpdateReservationRequest request, CancellationToken ct = default)
        {

            var reservation = await db.Reservations.FindAsync([id], ct);

            if (reservation == null)
            {
                throw NotFoundException.For<Reservation>(id);
            }

            if (reservation.Status != ReservationStatus.Booked)
            {
                throw new AppConflict("Reservation is cancelled or already finished");
            }



            await ValidateLocallyAsync(request.StartTime, request.EndTime, reservation.RoomId, reservation.UserId, request.AttendeeCount, ct, id);

            if (request.StartTime.HasValue) reservation.StartTime = request.StartTime.Value;
            if (request.EndTime.HasValue) reservation.EndTime = request.EndTime.Value;
            if (request.Title is not null) reservation.Title = request.Title;
            if (request.AttendeeCount.HasValue) reservation.AttendeeCount = request.AttendeeCount.Value;
            if (request.Status.HasValue)
            {
                if (request.Status.Value == ReservationStatus.Completed)
                {
                    reservation.ActualEndTime = DateTime.UtcNow;
                }
                reservation.Status = request.Status.Value;
            } 


            await db.SaveChangesAsync(ct);

            return await GetReservationByIdAsync(id, ct);
        }

        public async Task DeleteReservationAsync(int id, CancellationToken ct = default)
        {
            var reservation = await db.Reservations.FindAsync([id], ct);

            if (reservation == null)
            {
                throw NotFoundException.For<Reservation>(id);
            }
            db.Reservations.Remove(reservation);
            await db.SaveChangesAsync(ct);

        }
        private static readonly Expression<Func<Reservation, ReservationResponse>> ToResponse =
            r => new ReservationResponse
            {
                Id = r.Id,
                Title = r.Title,
                userId = r.UserId,
                UserName = r.User.Name,
                UserSurname = r.User.Surname,
                RoomId = r.RoomId,
                RoomName = r.Room.Name,
                StartTime = r.StartTime,
                EndTime = r.ActualEndTime ?? r.EndTime,
                AttendeeCount = r.AttendeeCount,
                Status = r.Status
            };

        private async Task ValidateLocallyAsync(DateTime? reqStartTime, DateTime? reqEndTime, int reqRoomId, int reqUserId, int? reqAttendeeCount, CancellationToken ct, int? reqReservationId = null )
        {
            if (reqStartTime < DateTime.UtcNow)
            {
                throw new AppValidationException("Start time can't be in the past");
            }
            if (reqEndTime <= reqStartTime)
            {
                throw new AppValidationException("End time must be after start time");
            }
            var duration = reqEndTime - reqStartTime;

            if(duration > TimeSpan.FromDays(rules.MaxDurationDays))
            {
                throw new AppValidationException($"A reservation can't be longer than {rules.MaxDurationDays} days");
            }

            if(reqStartTime > DateTime.UtcNow.AddDays(rules.MaxAdvanceReservationDays))
            {
                throw new AppValidationException($"A reservation can't take place later than  {rules.MaxAdvanceReservationDays} days");
            }

            var roomOverlaps = await db.Reservations.AnyAsync(r =>
                r.RoomId == reqRoomId &&
                r.Status == ReservationStatus.Booked &&
                r.Id != reqReservationId &&
                r.StartTime < reqEndTime &&
                r.EndTime > reqStartTime, ct
            );

            var room = await db.Rooms
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == reqRoomId, ct);
            
            if (room == null) throw new AppValidationException($"Room {reqRoomId} does not exist");

            if (roomOverlaps)
            {
                throw new AppValidationException($"Room {room.Name} is already booked for selected time");
            }

            if(room.Capacity < reqAttendeeCount)
            {
                throw new AppValidationException($"Attendee count {reqAttendeeCount} exceeds the room capacity of {room.Capacity}");
            }
            if(room.Status == RoomStatus.Inactive)
            {
                throw new AppValidationException($"Room {room.Name} is inactive");
            }

            var userOverlaps = await db.Reservations.AnyAsync( r =>
                r.UserId == reqUserId &&
                r.Status == ReservationStatus.Booked &&
                r.Id != reqReservationId &&
                r.StartTime < reqEndTime &&
                r.EndTime > reqStartTime, ct
            );

            if (userOverlaps)
            {
                throw new AppValidationException($"You already have a reservation for selected time");
            }
        }
    }
}