using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Bookings
{
    public class UpdateBookingRequest
    {
        public int RoomId { get; set; }

        public int UserId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}