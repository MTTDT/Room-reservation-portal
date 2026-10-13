using Backend.DTOs.Rooms;
using Backend.Models;
using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class RoomService(AppDbContext db) : IRoomService
    {
        public async Task<IReadOnlyList<RoomResponse>> GetAllRoomsAsync(CancellationToken ct = default)
        {
            return await db.Rooms
                .AsNoTracking()
                .OrderBy(r => r.Id)
                .Select(r => ToRoomResponse(r))
                .ToListAsync(ct);
        }

        public async Task<RoomResponse> GetRoomByIdAsync(int id, CancellationToken ct = default)
        {
            var room = await db.Rooms
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id, ct);

            if (room == null)
            {
                return null;
            }

            return ToRoomResponse(room);
        }

        public async Task<RoomResponse> CreateRoomAsync(CreateRoomRequest request, CancellationToken ct = default)
        {
            var room = new Room
            {
                Name = request.Name.Trim(),
                Capacity = request.Capacity,
                Floor = request.Floor,
                Location = request.Location.Trim(),
                Status = request.Status,
                Equipment = request.Equipment?.ToList() ?? []
            };

            db.Rooms.Add(room);
            await db.SaveChangesAsync(ct);

            return ToRoomResponse(room);
        }
        public async Task<RoomResponse> UpdateRoomAsync(int id, UpdateRoomRequest request, CancellationToken ct = default)
        {
            var room = await db.Rooms.FindAsync([id], ct);

            if (room == null)
            {
                return null;
            }

            room.Name = request.Name.Trim();
            room.Capacity = request.Capacity;
            room.Floor = request.Floor;
            room.Location = request.Location.Trim();
            room.Status = request.Status;
            room.Equipment = request.Equipment?.ToList() ?? [];

            await db.SaveChangesAsync(ct);

            return ToRoomResponse(room);
        }

        public async Task DeleteRoomAsync(int id, CancellationToken ct = default)
        {
            var room = await db.Rooms.FindAsync(new object[] { id }, ct);

            if (room == null)
            {
                return;
            }

            db.Rooms.Remove(room);
            await db.SaveChangesAsync(ct);
        }

        private static RoomResponse ToRoomResponse(Room room) => new()
        {
            Id = room.Id,
            Name = room.Name,
            Capacity = room.Capacity,
            Floor = room.Floor,
            Location = room.Location,
            Status = room.Status,
            Equipment = room.Equipment.Select(e => e.ToString()).ToList()
        };
    }
}
        
        