using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.DTOs.Reservations
{
    public class FilterReservationRequest
    {
        [FromQuery(Name = "room-name")]
        public string? RoomName { get; set; }

        [FromQuery(Name = "organizer")]
        public string? Organizer { get; set; }

        [FromQuery(Name = "status")]

        public ReservationStatus? Status { get; set; }

        [FromQuery(Name = "from")]
        public DateTime? From { get; set; }

        [FromQuery(Name = "to")]
        public DateTime? To { get; set; }
    }
}