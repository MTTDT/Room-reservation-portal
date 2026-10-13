using Backend.Models;
using Backend.Data;
using Backend.DTOs.Bookings;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class BookingService(AppDbContext db) : IBookingService
    {
        public async Task<IReadOnlyList<BookingResponse>> GetAllBookingsAsync(CancellationToken ct = default)
        {
            return await db.Bookings
                .AsNoTracking()
                .OrderBy(b => b.Id)
                .Select(b => ToBookingResponse(b))
                .ToListAsync(ct);
        }

        public async Task<BookingResponse> GetBookingByIdAsync(int id, CancellationToken ct = default)
        {
            var booking = await db.Bookings
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id, ct);

            if (booking == null)
            {
                return null;
            }

            return ToBookingResponse(booking);
        }

        public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request, CancellationToken ct = default)
        {
            var booking = new Booking
            {
                RoomId = request.RoomId,
                UserId = request.UserId,
                StartTime = request.StartTime,
                EndTime = request.EndTime
            };

            db.Bookings.Add(booking);
            await db.SaveChangesAsync(ct);

            return ToBookingResponse(booking);
        }

        public async Task<BookingResponse> UpdateBookingAsync(int id, UpdateBookingRequest request, CancellationToken ct = default)
        {
            var booking = await db.Bookings.FindAsync(new object[] { id }, ct);

            if (booking == null)
            {
                return null;
            }

            booking.RoomId = request.RoomId;
            booking.UserId = request.UserId;
            booking.StartTime = request.StartTime;
            booking.EndTime = request.EndTime;

            await db.SaveChangesAsync(ct);

            return ToBookingResponse(booking);
        }

        public async Task DeleteBookingAsync(int id, CancellationToken ct = default)
        {
            var booking = await db.Bookings.FindAsync(new object[] { id }, ct);

            if (booking != null)
            {
                db.Bookings.Remove(booking);
                await db.SaveChangesAsync(ct);
            }
        }
        private static BookingResponse ToBookingResponse(Booking booking) => new()
        {
            Id = booking.Id,
            UserId = booking.UserId,
            RoomId = booking.RoomId,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime
        };
    }
}