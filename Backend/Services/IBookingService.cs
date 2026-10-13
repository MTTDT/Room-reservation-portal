using Backend.DTOs.Bookings;

namespace Backend.Services
{
    public interface IBookingService
    {
        Task<IReadOnlyList<BookingResponse>> GetAllBookingsAsync(CancellationToken ct = default);
        Task<BookingResponse> GetBookingByIdAsync(int id, CancellationToken ct = default);
        Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request, CancellationToken ct = default);
        Task<BookingResponse> UpdateBookingAsync(int id, UpdateBookingRequest request, CancellationToken ct = default);
        Task DeleteBookingAsync(int id, CancellationToken ct = default);
    }
}