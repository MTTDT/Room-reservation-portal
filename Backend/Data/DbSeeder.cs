using Backend.Models;


namespace Backend.Data
{
    public class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
                        if (db.Users.Any()) return;
 
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
                new() { Name = "Ava", Surname = "Martin" },
                new() { Name = "Liam", Surname = "Garcia" },
                new() { Name = "Mia", Surname = "Rodriguez" } 
            };
 
            var rooms = new List<Room>
            {
                new() { Name = "Conference Room A", Capacity = 10, Floor = 1, Location = "Building 1", Status = RoomStatus.Active,   Equipment = new List<RoomEquipment> { RoomEquipment.Projector, RoomEquipment.Whiteboard, RoomEquipment.VideoConferencing } },
                new() { Name = "Meeting Room B",    Capacity = 5,  Floor = 2, Location = "Building 2", Status = RoomStatus.Active,   Equipment = new List<RoomEquipment> { RoomEquipment.Projector } },
                new() { Name = "Training Room C",   Capacity = 20, Floor = 3, Location = "Building 3", Status = RoomStatus.Active,   Equipment = new List<RoomEquipment> { RoomEquipment.Whiteboard, RoomEquipment.Projector } }, // 2
                new() { Name = "Board Room D",      Capacity = 15, Floor = 4, Location = "Building 4", Status = RoomStatus.Active,   Equipment = new List<RoomEquipment> { RoomEquipment.Projector, RoomEquipment.Whiteboard, RoomEquipment.VideoConferencing } }, // 3
                new() { Name = "Huddle Room E",     Capacity = 4,  Floor = 1, Location = "Building 1", Status = RoomStatus.Active,   Equipment = new List<RoomEquipment> { RoomEquipment.Whiteboard } },  // 4
                new() { Name = "Conference Room F", Capacity = 12, Floor = 2, Location = "Building 2", Status = RoomStatus.Active,   Equipment = new List<RoomEquipment> { RoomEquipment.Projector, RoomEquipment.VideoConferencing } }, // 5
                new() { Name = "Meeting Room G",    Capacity = 6,  Floor = 3, Location = "Building 3", Status = RoomStatus.Active,   Equipment = new List<RoomEquipment> { RoomEquipment.Projector, RoomEquipment.Whiteboard } }, // 6
                new() { Name = "Workshop Room H",   Capacity = 25, Floor = 1, Location = "Building 4", Status = RoomStatus.Inactive, Equipment = new List<RoomEquipment> { RoomEquipment.Whiteboard, RoomEquipment.Projector } },
                new() { Name = "Executive Room I",  Capacity = 8,  Floor = 5, Location = "Building 1", Status = RoomStatus.Active,   Equipment = new List<RoomEquipment> { RoomEquipment.VideoConferencing, RoomEquipment.Projector } },
                new() { Name = "Quiet Room J",      Capacity = 4,  Floor = 2, Location = "Building 3", Status = RoomStatus.Active,   Equipment = new List<RoomEquipment>() } // 9
            };
 
            db.Users.AddRange(users);
            db.Rooms.AddRange(rooms);
            db.SaveChanges();
 
            var now = DateTime.Now;
            var today = DateTime.Today;
 
            // Status is derived from time unless given explicitly:
            // already finished => Completed, otherwise => Booked.
            // endedEarlyMin shortens a Completed meeting to simulate "end meeting early".
            Reservation R(int room, string title, int user, int dayOffset, int startHour, int hours,
                          int attendees, ReservationStatus? status = null, int endedEarlyMin = 0)
            {
                var start = today.AddDays(dayOffset).AddHours(startHour);
                var end = start.AddHours(hours);
                var resolved = status ?? (end <= now ? ReservationStatus.Completed : ReservationStatus.Booked);
                if (resolved == ReservationStatus.Completed && endedEarlyMin > 0)
                    end = end.AddMinutes(-endedEarlyMin);
 
                return new()
                {
                    RoomId = rooms[room].Id,
                    UserId = users[user].Id,
                    Title = title,
                    StartTime = start,
                    EndTime = end,
                    AttendeeCount = attendees,
                    Status = resolved
                };
            }
 
            const ReservationStatus X = ReservationStatus.Cancelled;
 
            // Usage story for the analytics view:
            //  - Conference Room A (cap 10) and Conference Room F (cap 12) are booked constantly,
            //    but with 2-4 attendees, many cancellations and early-ended meetings.
            //  - Board Room D is booked for big meetings but often used by only a few people.
            //  - Huddle E, Meeting G, Training C are booked and well utilised.
            //  - Quiet Room J is almost never booked; Workshop Room H is inactive (renovation).
            // No two non-cancelled reservations overlap for the same room or the same organizer.
            var reservations = new List<Reservation>
            {
                // ---- Past: two weeks ago (Tue-Fri) ----
                R(0, "Sprint planning", 1, -14, 9, 2, 4),
                R(5, "Quarterly business review", 3, -14, 10, 2, 3),
                R(2, "New hire onboarding", 4, -14, 13, 3, 12),
                R(3, "Board prep", 2, -14, 14, 1, 2),
                R(0, "Design sync", 5, -14, 13, 1, 3),
                R(4, "1:1 catch-up", 6, -14, 11, 1, 2),
                R(6, "Vendor demo", 7, -14, 15, 1, 5, X),
 
                R(0, "Product roadmap review", 0, -13, 9, 2, 3, endedEarlyMin: 30),
                R(1, "Candidate interview", 3, -13, 10, 1, 2),
                R(6, "Marketing brainstorm", 5, -13, 13, 2, 5),
                R(5, "Customer call with ACME", 1, -13, 15, 1, 2),
                R(8, "Executive sync", 9, -13, 10, 1, 4),
                R(0, "Retro", 2, -13, 14, 1, 3),
                R(0, "Budget workshop", 4, -13, 11, 2, 8, X),
 
                R(0, "Weekly standup", 6, -12, 9, 1, 3),
                R(3, "Strategy offsite prep", 8, -12, 10, 2, 3),
                R(5, "Partner workshop", 0, -12, 13, 2, 4),
                R(2, "Security training", 7, -12, 9, 3, 15),
                R(4, "Pair programming", 5, -12, 14, 1, 2),
                R(0, "Interview panel", 3, -12, 11, 2, 2),
                R(0, "Hiring debrief", 1, -12, 14, 1, 3),
                R(5, "Lunch & learn", 2, -12, 10, 1, 6, X),
 
                R(0, "Demo day", 4, -11, 15, 2, 8),
                R(6, "Retro", 2, -11, 10, 1, 5),
                R(1, "Support sync", 7, -11, 11, 1, 4),
                R(5, "All-hands rehearsal", 9, -11, 9, 1, 3),
                R(8, "Investor update", 1, -11, 14, 2, 5),
                R(0, "Sprint review", 3, -11, 10, 1, 6, X),
 
                // ---- Past: last week ----
                R(0, "Sprint planning", 1, -8, 9, 2, 4, endedEarlyMin: 30),
                R(5, "Sales pipeline review", 6, -8, 10, 1, 3),
                R(3, "Board meeting", 8, -8, 13, 3, 12),
                R(4, "Quick sync", 0, -8, 10, 1, 3),
                R(2, "Leadership workshop", 9, -8, 9, 4, 14),
                R(1, "Candidate interview", 3, -8, 14, 1, 2),
                R(0, "Brainstorm", 5, -8, 13, 2, 3),
                R(9, "Focus session", 2, -8, 9, 2, 1, X),
                R(7, "Design workshop", 5, -8, 10, 3, 15, X),   // cancelled: room closed for renovation
 
                R(0, "Architecture review", 0, -7, 10, 2, 5, endedEarlyMin: 60),
                R(6, "Finance review", 4, -7, 9, 2, 4),
                R(5, "Client workshop", 2, -7, 13, 3, 9),
                R(3, "Compliance briefing", 7, -7, 10, 1, 2),
                R(8, "Exec sync", 9, -7, 9, 1, 4),
                R(0, "Skip-level 1:1", 3, -7, 13, 1, 2),
                R(4, "Design critique", 5, -7, 15, 1, 4),
                R(0, "Team social", 8, -7, 14, 2, 10, X),
                R(3, "Planning offsite", 1, -7, 14, 2, 10, X),
 
                R(0, "Product demo", 5, -6, 9, 1, 3),
                R(6, "Hiring sync", 3, -6, 10, 1, 4),
                R(1, "Customer interview", 6, -6, 13, 1, 2),
                R(2, "OKR workshop", 9, -6, 10, 3, 16),
                R(5, "Roadmap alignment", 1, -6, 14, 2, 3),
                R(0, "Daily scrum", 2, -6, 11, 1, 3),
                R(4, "Mentoring", 0, -6, 13, 1, 2),
                R(0, "Quarterly kickoff", 7, -6, 9, 1, 9, X),
 
                R(0, "Interview loop", 3, -5, 9, 3, 2, endedEarlyMin: 90),
                R(3, "Legal review", 8, -5, 13, 2, 3),
                R(8, "Investor call", 1, -5, 10, 1, 3),
                R(6, "Sprint planning", 4, -5, 10, 2, 6),
                R(5, "Training session", 7, -5, 13, 2, 11),
                R(0, "Brainstorm", 6, -5, 13, 1, 3),
                R(2, "Onboarding", 0, -5, 9, 2, 8),
                R(1, "Retro", 5, -5, 15, 1, 5),
                R(4, "Standup", 2, -5, 9, 1, 4, X),
 
                R(0, "Weekly demo", 2, -4, 14, 1, 4),
                R(5, "Town hall prep", 9, -4, 10, 1, 2),
                R(6, "Retro", 3, -4, 11, 1, 5),
                R(2, "Brown-bag lunch", 7, -4, 12, 2, 18),
                R(1, "Support triage", 6, -4, 9, 1, 3),
                R(3, "Board prep", 8, -4, 9, 2, 4),
                R(0, "Offsite planning", 1, -4, 9, 2, 10, X),
 
                // ---- Yesterday ----
                R(0, "Sprint planning", 1, -1, 9, 2, 4, endedEarlyMin: 45),
                R(5, "Customer success sync", 0, -1, 13, 1, 3),
                R(3, "Strategy session", 8, -1, 10, 3, 9),
                R(6, "Hiring debrief", 3, -1, 10, 1, 4),
                R(4, "Pair programming", 5, -1, 13, 2, 2),
                R(8, "Executive staff", 9, -1, 14, 1, 5),
                R(1, "Design review", 4, -1, 9, 1, 4),
                R(0, "Team sync", 6, -1, 13, 1, 3),
                R(0, "Q4 planning", 2, -1, 11, 2, 9, X),
                R(2, "Workshop", 7, -1, 13, 3, 16, X),
 
                // ---- Today (status depends on current time) ----
                R(0, "Sprint planning", 1, 0, 9, 2, 4, endedEarlyMin: 30),
                R(5, "Weekly sync", 0, 0, 9, 1, 3),
                R(3, "Board meeting", 8, 0, 10, 2, 10),
                R(6, "Hiring debrief", 3, 0, 11, 1, 4),
                R(4, "Pair programming", 5, 0, 13, 2, 2),
                R(0, "Customer demo", 2, 0, 13, 2, 3),
                R(8, "Exec sync", 9, 0, 14, 1, 5),
                R(2, "Quarterly training", 7, 0, 14, 3, 14),
                R(1, "Design review", 4, 0, 15, 1, 3),
                R(5, "Product roadmap", 6, 0, 15, 2, 6),
                R(9, "Focus time", 8, 0, 15, 2, 1),
                R(0, "Retro", 0, 0, 16, 1, 5),
                R(0, "Budget review", 3, 0, 15, 1, 8, X),
 
                // ---- Upcoming ----
                R(0, "Sprint planning", 1, 1, 9, 2, 4),
                R(5, "Customer workshop", 0, 1, 10, 2, 6),
                R(3, "Board meeting", 8, 1, 13, 2, 10),
                R(2, "Security awareness", 7, 1, 9, 2, 18),
                R(6, "Design review", 5, 1, 10, 1, 4),
                R(4, "1:1", 3, 1, 11, 1, 2),
                R(1, "Candidate interview", 4, 1, 14, 1, 3),
                R(8, "Exec sync", 9, 1, 10, 1, 5),
                R(0, "Team sync", 6, 1, 13, 1, 3),
                R(0, "Offsite prep", 2, 1, 9, 2, 8, X),
 
                R(0, "Product demo", 2, 2, 10, 1, 4),
                R(5, "Sales review", 6, 2, 9, 2, 5),
                R(6, "Hiring panel", 3, 2, 13, 2, 5),
                R(2, "Leadership offsite", 9, 2, 9, 6, 16),
                R(4, "Pairing session", 5, 2, 10, 2, 2),
                R(3, "Legal review", 8, 2, 10, 1, 3),
                R(1, "Customer call", 0, 2, 14, 1, 3),
                R(9, "Quiet focus", 4, 2, 9, 3, 1),
 
                R(0, "Weekly demo", 1, 3, 14, 1, 6),
                R(5, "Town hall", 9, 3, 10, 2, 12),
                R(2, "Training workshop", 7, 3, 13, 2, 15),
                R(6, "Retro", 3, 3, 10, 1, 5),
                R(1, "Candidate interview", 4, 3, 11, 1, 2),
                R(8, "Investor update", 8, 3, 10, 2, 5),
 
                R(0, "Q4 planning", 2, 6, 9, 3, 9),
                R(3, "Strategy session", 8, 6, 10, 3, 12),
                R(5, "Customer success", 0, 6, 13, 1, 4),
                R(6, "Hiring sync", 3, 6, 11, 1, 4),
                R(4, "Design critique", 5, 6, 13, 1, 4),
                R(1, "Support sync", 6, 6, 9, 1, 3),
 
                R(0, "Architecture review", 0, 7, 10, 2, 6),
                R(2, "Onboarding", 4, 7, 9, 3, 10),
                R(5, "Client workshop", 1, 7, 14, 2, 8),
                R(8, "Exec staff", 9, 7, 10, 1, 6),
                R(6, "Finance review", 3, 7, 13, 2, 5),
 
                R(0, "Brainstorm", 6, 8, 13, 2, 5),
                R(3, "Compliance briefing", 7, 8, 10, 2, 6),
                R(4, "Mentoring", 5, 8, 9, 1, 2),
 
                R(0, "Demo day", 4, 10, 15, 2, 9),
                R(5, "Partner workshop", 2, 10, 10, 3, 8),
 
                R(0, "Sprint planning", 1, 14, 9, 2, 4),
                R(3, "Board meeting", 8, 14, 13, 3, 12),
 
                // ---- Long-range bookings (7-day max, 90-day horizon) ----
                R(2, "Company hackathon", 9, 20, 8, 168, 20),     // exactly 7 days
                R(5, "Annual planning", 0, 30, 9, 8, 10),
                R(0, "Customer summit prep", 3, 30, 10, 2, 8),
                R(3, "Board offsite", 8, 45, 9, 6, 14),
                R(2, "Training bootcamp", 7, 60, 9, 72, 18),     // 3 days
                R(8, "Year-end review", 9, 85, 10, 3, 6)
            };
        

                db.Reservations.AddRange(reservations);
                db.SaveChanges();

            
        }
    }
}