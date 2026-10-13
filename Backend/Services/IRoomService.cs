using Backend.DTOs.Rooms;

namespace Backend.Services
{
    public interface IRoomService
    {
        Task<IReadOnlyList<RoomResponse>> GetAllRoomsAsync(CancellationToken ct = default);
        Task<RoomResponse> GetRoomByIdAsync(int id, CancellationToken ct = default);
        Task<RoomResponse> CreateRoomAsync(CreateRoomRequest request, CancellationToken ct = default);
        Task<RoomResponse> UpdateRoomAsync(int id, UpdateRoomRequest request, CancellationToken ct = default);
        Task DeleteRoomAsync(int id, CancellationToken ct = default);
    }
}