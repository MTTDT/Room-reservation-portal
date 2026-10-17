using Backend.DTOs.Users;
using Backend.Models;

namespace Backend.DTOs.Reservations
{
    public record ReservationResponse
    {
        public int Id { get; set; }
        public string Title {get; set;} = string.Empty;
        public int userId {get; set;}
        public string UserName { get; set; } = string.Empty;
        public string UserSurname {get; set; } = string.Empty;
        public int RoomId {get; set;}
        public string RoomName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int AttendeeCount {get; set;} = 0;
        public ReservationStatus Status {get; set;}
        
    }
}