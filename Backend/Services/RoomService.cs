using Backend.DTOs.Rooms;
using Backend.Models;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Exceptions;
using Backend.DTOs.Users;

namespace Backend.Services
{
    public class RoomService(AppDbContext db) : IRoomService
    {
        public async Task<IReadOnlyList<RoomResponse>> GetAllRoomsAsync(FilterRoomRequest? filter = null,  CancellationToken ct = default)
        {
            IQueryable<Room> query = db.Rooms.AsNoTracking();

            if(filter == null)
            {
                return await query
                    .OrderBy(r => r.Id)
                    .Select(r => ToRoomResponse(r))
                    .ToListAsync(ct);
            }

            if(filter.Capacity.HasValue) query = query.Where(r => r.Capacity >= filter.Capacity);

            if(filter.Floor.HasValue) query = query.Where(r => r.Floor == filter.Floor);

            var rooms = await query
                .OrderBy(r => r.Id)
                .ToListAsync(ct);
          
            if(filter.Equipment != null)
            {
                var equipmentFilter = filter.Equipment.ToList();
                rooms = rooms.Where(r => equipmentFilter.All(r.Equipment.Contains)).ToList();
            } 

            
             return rooms
                .Select(ToRoomResponse)
                .ToList(); 
        }

        public async Task<RoomResponse> GetRoomByIdAsync(int id, CancellationToken ct = default)
        {
            var room = await db.Rooms
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id, ct);
            if (room == null)
            {
                throw NotFoundException.For<Room>(id);
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
                throw NotFoundException.For<Room>(id);
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
            var room = await db.Rooms.FindAsync([id], ct);

            if (room == null)
            {
                throw NotFoundException.For<Room>(id);
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
        
        