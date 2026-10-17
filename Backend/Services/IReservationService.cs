using Backend.DTOs.Reservations;

namespace Backend.Services
{
    public interface IReservationService
    {
        Task<IReadOnlyList<ReservationResponse>> GetAllReservationsAsync(CancellationToken ct = default);
        Task<ReservationResponse> GetReservationByIdAsync(int id, CancellationToken ct = default);
        Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request, CancellationToken ct = default);
        Task<ReservationResponse> UpdateReservationAsync(int id, UpdateReservationRequest request, CancellationToken ct = default);
        Task DeleteReservationAsync(int id, CancellationToken ct = default);
    }
}