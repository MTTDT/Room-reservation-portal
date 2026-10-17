using System.Linq.Expressions;
using Backend.Models;
using Backend.Data;
using Backend.DTOs.Reservations;
using Microsoft.EntityFrameworkCore;
using Backend.DTOs.Users;

namespace Backend.Services
{
    public class ReservationService(AppDbContext db) : IReservationService
    {
        public async Task<IReadOnlyList<ReservationResponse>> GetAllReservationsAsync(CancellationToken ct = default)
        {
            return await db.Reservations
                .AsNoTracking()
                .OrderBy(r => r.Id)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<ReservationResponse> GetReservationByIdAsync(int id, CancellationToken ct = default)
        {
            return await db.Reservations
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);

            
        }

        public async Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request, CancellationToken ct = default)
        {
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
            var reservation = await db.Reservations.FindAsync(new object[] { id }, ct);

            if (reservation == null)
            {
                return null;
            }

            reservation.StartTime = request.StartTime;
            reservation.EndTime = request.EndTime;

            await db.SaveChangesAsync(ct);

            return await GetReservationByIdAsync(id, ct);
        }

        public async Task DeleteReservationAsync(int id, CancellationToken ct = default)
        {
            var reservation = await db.Reservations.FindAsync(new object[] { id }, ct);

            if (reservation != null)
            {
                db.Reservations.Remove(reservation);
                await db.SaveChangesAsync(ct);
            }
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
                EndTime = r.EndTime,
                AttendeeCount = r.AttendeeCount,
                Status = r.Status
            };
    }
}