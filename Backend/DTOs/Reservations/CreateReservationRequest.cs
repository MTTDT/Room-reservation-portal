using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.DTOs.Reservations
{
    public class CreateReservationRequest
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Title {get; set;} = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public int AttendeeCount {get; set;} = 0;

        
    }
}