using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.DTOs.Reservations
{
    public class UpdateReservationRequest
    {
        
        public string? Title {get; set;} = string.Empty;

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int? AttendeeCount {get; set;}
        
        public ReservationStatus? Status {get; set;}
    }
}