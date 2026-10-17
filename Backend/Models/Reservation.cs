namespace Backend.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string Title {get; set;} = string.Empty;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int AttendeeCount {get; set;} = 0;
        public DateTime? ActualEndTime { get; set; }
        public ReservationStatus Status {get; set; } = ReservationStatus.Booked;
    }
}