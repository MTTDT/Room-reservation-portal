using Backend.DTOs.Rooms;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController(IRoomService roomService) : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> GetAllRooms(CancellationToken ct = default)
        {
            var rooms = await roomService.GetAllRoomsAsync(ct);
            return Ok(rooms);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetRoomById(int id, CancellationToken ct = default)
        {
            var room = await roomService.GetRoomByIdAsync(id, ct);
            if (room == null)
            {
                return NotFound();
            }
            return Ok(room);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom(CreateRoomRequest request, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var createdRoom = await roomService.CreateRoomAsync(request, ct);
            return CreatedAtAction(nameof(GetRoomById), new { id = createdRoom.Id }, createdRoom);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRoom(int id, UpdateRoomRequest request, CancellationToken ct = default)
        {
            var updatedRoom = await roomService.UpdateRoomAsync(id, request, ct);
            if (updatedRoom == null)
            {
                return NotFound();
            }
            return Ok(updatedRoom);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRoom(int id, CancellationToken ct = default)
        {
            await roomService.DeleteRoomAsync(id, ct);
            return NoContent();
        }
    }
}