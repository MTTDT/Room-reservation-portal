using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Reservations
{
    public class UpdateReservationRequest
    {
        public string Title {get; set;} = string.Empty;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int AttendeeCount {get; set;}
    }
}