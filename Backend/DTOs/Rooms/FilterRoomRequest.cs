using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.DTOs.Rooms
{
    public class FilterRoomRequest
    {
        [FromQuery(Name = "capacity")]
        public int? Capacity { get; set; }

        [FromQuery(Name = "equipment")]
        public List<RoomEquipment>? Equipment { get; set; }

        [FromQuery(Name = "floor")]
        public int? Floor { get; set; }
    }
}