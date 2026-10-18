using Backend.Models;
using Backend.DTOs.Users;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Exceptions;

namespace Backend.Services
{
    public class UserService(AppDbContext db) : IUserService
    {
        public async Task<IReadOnlyList<UserResponse>> GetAllUsersAsync(CancellationToken ct = default)
        {
            var now = DateTime.Now;
            return await db.Users
                    .AsNoTracking()
                    .OrderBy(u => u.Id)
                    .Select(u => new UserResponse
                        {
                            Id = u.Id,
                            Name = u.Name,
                            Surname = u.Surname,
                            ActiveRegistrations = db.Reservations.Count(r => r.UserId == u.Id && r.EndTime > now)
                        }
                    
                    )
                    .ToListAsync(ct);
        }

        public async Task<UserResponse> GetUserByIdAsync(int id, CancellationToken ct = default)
        {
            var user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, ct);

            if (user == null)
            {
                throw NotFoundException.For<User>(id);
            }

            return ToUserResponse(user);
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
        {
            var user = new User
            {
                Name = request.Name.Trim(),
                Surname = request.Surname.Trim()
            };

            db.Users.Add(user);
            await db.SaveChangesAsync(ct);

            return ToUserResponse(user);
        }

        public async Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken ct = default)
        {
            var user = await db.Users.FindAsync(new object[] { id }, ct);

            if (user == null)
            {
                throw NotFoundException.For<User>(id);
            }

            if (request.Name != null)
            {
                user.Name = request.Name.Trim();
            }

            if (request.Surname != null)
            {
                user.Surname = request.Surname.Trim();
            }

            await db.SaveChangesAsync(ct);

            return ToUserResponse(user);
        }

        public async Task DeleteUserAsync(int id, CancellationToken ct = default)
        {
            var user = await db.Users.FindAsync([id], ct);

            if(user == null)
            {
                throw NotFoundException.For<User>(id);
            }
          
            db.Users.Remove(user);
            await db.SaveChangesAsync(ct);
        }

        private static UserResponse ToUserResponse(User user) => new()
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname
        };
           
        
    }
}