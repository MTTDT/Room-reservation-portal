using Backend.Models;

namespace Backend.Data
{
    public class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            if (!db.Users.Any())
            {
                var users = new List<User>
                {
                    new User { Name = "John", Surname = "Doe" },
                    new User { Name = "Jane", Surname = "Smith" }
                };

                var rooms = new List<Room>
                {
                    new() { Name = "Conference Room A", Capacity = 10, Floor = 1, Location = "Building 1", Status = RoomStatus.Active, Equipment = new List<RoomEquipment> { RoomEquipment.Projector, RoomEquipment.Whiteboard, RoomEquipment.VideoConferencing } },
                    new() { Name = "Meeting Room B", Capacity = 5, Floor = 2, Location = "Building 2", Status = RoomStatus.Active, Equipment = new List<RoomEquipment> { RoomEquipment.Projector } },
                    new() { Name = "Training Room C", Capacity = 20, Floor = 3, Location = "Building 3", Status = RoomStatus.Active, Equipment = new List<RoomEquipment> { RoomEquipment.Whiteboard } },
                    new() { Name = "Board Room D", Capacity = 15, Floor = 4, Location = "Building 4", Status = RoomStatus.Active, Equipment = new List<RoomEquipment> { RoomEquipment.Projector, RoomEquipment.Whiteboard } }
                };

                db.Users.AddRange(users);
                db.Rooms.AddRange(rooms);
                db.SaveChanges();
            }
        }
    }
}