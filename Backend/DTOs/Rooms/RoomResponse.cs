using Backend.Models;
namespace Backend.DTOs.Rooms
{
    public record RoomResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int Floor { get; set; }
        public string Location { get; set; } = string.Empty;
        public RoomStatus Status { get; set; } = RoomStatus.Active;
        public List<string> Equipment { get; set; } = [];
    }
}