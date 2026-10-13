namespace Backend.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int Floor { get; set; }
        public string Location { get; set; } = string.Empty;
        public RoomStatus Status { get; set; } = RoomStatus.Active;
        public List<RoomEquipment> Equipment { get; set; } = [];
    }
}