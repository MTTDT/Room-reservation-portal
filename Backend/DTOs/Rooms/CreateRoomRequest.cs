using Backend.Models;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Rooms
{
    public class CreateRoomRequest
    {
        [Required, StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required, Range(1, 1000)]
        public int Capacity { get; set; }

        [Required]
        public int Floor { get; set; }

        [Required, StringLength(200, MinimumLength = 2)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public RoomStatus Status { get; set; } = RoomStatus.Active;

        [Required]
        public List<RoomEquipment> Equipment { get; set; } = [];
        
    }
}