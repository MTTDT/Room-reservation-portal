using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Bookings
{
    public class CreateBookingRequest
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        
    }
}