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
                    new() { Name = "John", Surname = "Doe" },
                    new() { Name = "Jane", Surname = "Smith" },
                    new() { Name = "Michael", Surname = "Johnson" },
                    new() { Name = "Emily", Surname = "Brown" },
                    new() { Name = "Daniel", Surname = "Wilson" },
                    new() { Name = "Olivia", Surname = "Taylor" },
                    new() { Name = "James", Surname = "Anderson" },
                    new() { Name = "Sophia", Surname = "Thomas" },
                    new() { Name = "William", Surname = "Moore" },
                    new() { Name = "Ava", Surname = "Martin" }
                };

                var rooms = new List<Room>
                {
                    new() { Name = "Conference Room A", Capacity = 10, Floor = 1, Location = "Building 1", Status = RoomStatus.Active, Equipment = new List<RoomEquipment> { RoomEquipment.Projector, RoomEquipment.Whiteboard, RoomEquipment.VideoConferencing } },
                    new() { Name = "Meeting Room B", Capacity = 5, Floor = 2, Location = "Building 2", Status = RoomStatus.Active, Equipment = new List<RoomEquipment> { RoomEquipment.Projector } },
                    new() { Name = "Training Room C", Capacity = 20, Floor = 3, Location = "Building 3", Status = RoomStatus.Active, Equipment = new List<RoomEquipment> { RoomEquipment.Whiteboard } },
                    new() { Name = "Board Room D", Capacity = 15, Floor = 4, Location = "Building 4", Status = RoomStatus.Active, Equipment = new List<RoomEquipment> { RoomEquipment.Projector, RoomEquipment.Whiteboard } },
                    new() { Name = "Huddle Room E", Capacity = 4, Floor = 1, Location = "Building 1", Status = RoomStatus.Active, Equipment = new List<RoomEquipment> { RoomEquipment.Whiteboard } },
                    new() { Name = "Conference Room F", Capacity = 12, Floor = 2, Location = "Building 2", Status = RoomStatus.Active, Equipment = new List<RoomEquipment> { RoomEquipment.Projector, RoomEquipment.VideoConferencing } },
                    new() { Name = "Meeting Room G", Capacity = 6, Floor = 3, Location = "Building 3", Status = RoomStatus.Active, Equipment = new List<RoomEquipment> { RoomEquipment.Projector, RoomEquipment.Whiteboard } },
                    new() { Name = "Workshop Room H", Capacity = 25, Floor = 1, Location = "Building 4", Status = RoomStatus.Active, Equipment = new List<RoomEquipment> { RoomEquipment.Whiteboard, RoomEquipment.Projector } },
                    new() { Name = "Executive Room I", Capacity = 8, Floor = 5, Location = "Building 1", Status = RoomStatus.Active, Equipment = new List<RoomEquipment> { RoomEquipment.VideoConferencing, RoomEquipment.Projector } },
                    new() { Name = "Quiet Room J", Capacity = 4, Floor = 2, Location = "Building 3", Status = RoomStatus.Active, Equipment = new List<RoomEquipment>() }
                };

                db.Users.AddRange(users);
                db.Rooms.AddRange(rooms);
                db.SaveChanges();

                var today = DateTime.Today;

                Reservation R(int room, string title, int user, int dayOffset, int startHour, int hours, int attendeeCount, ReservationStatus status) => new()
                {
                    RoomId = rooms[room].Id,
                    Title = title,
                    UserId = users[user].Id,
                    StartTime = today.AddDays(dayOffset).AddHours(startHour),
                    EndTime = today.AddDays(dayOffset).AddHours(startHour + hours),
                    AttendeeCount = attendeeCount,
                    Status = status

                };

                var reservations = new List<Reservation>
                {
                    R(0, "Title0", 0, 0,  9, 2, 2, ReservationStatus.Booked),
                    R(0, "Title3", 0, 0, 13, 1, 0, ReservationStatus.Booked),
                    R(1, "Title1", 0, 0, 10, 1, 1, ReservationStatus.Booked),
                    R(2, "Title2", 0, 0, 14, 2, 3, ReservationStatus.Booked),
                    R(3, "Title4", 1, 0,  9, 3, 0, ReservationStatus.Booked),
                    R(0, "Title5", 1, 0, 11, 2, 1, ReservationStatus.Booked),
                    R(1, "Title6", 1, 0, 15, 1, 0, ReservationStatus.Booked),
                    R(4, "Title7", 1, 0, 10, 2, 1, ReservationStatus.Booked),
                    R(5, "Title8", 2, 0,  8, 2, 4, ReservationStatus.Booked),
                    R(0, "Title9", 2, 0, 10, 1, 0, ReservationStatus.Booked),
                    R(2, "Title0", 2, 0, 13, 3, 5, ReservationStatus.Booked),
                    R(6, "Title1", 3, 0,  9, 1, 2, ReservationStatus.Booked),
                    R(7, "Title2", 3, 0, 11, 2, 6, ReservationStatus.Booked),
                    R(0, "Title3", 3, 0, 14, 2, 1, ReservationStatus.Booked),
                    R(8, "Title4", 4, 0, 10, 2, 0, ReservationStatus.Booked),
                    R(9, "Title5", 4, 0, 13, 1, 0, ReservationStatus.Booked),
                    R(1, "Title6", 5, 0,  9, 2, 0, ReservationStatus.Booked),
                    R(3, "Title7", 5, 0, 14, 2, 2, ReservationStatus.Booked),
                    R(0, "Title8", 6, 0,  9, 3, 3, ReservationStatus.Booked),
                    R(4, "Title9", 6, 0, 13, 2, 1, ReservationStatus.Booked)
                };

                db.Reservations.AddRange(reservations);
                db.SaveChanges();

            }
        }
    }
}