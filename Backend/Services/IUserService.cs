using Backend.DTOs.Users;

namespace Backend.Services
{
    public interface IUserService
    {
        Task<IReadOnlyList<UserResponse>> GetAllUsersAsync(CancellationToken ct = default);
        Task<UserResponse> GetUserByIdAsync(int id, CancellationToken ct = default);
        Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default);
        Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken ct = default);
        Task DeleteUserAsync(int id, CancellationToken ct = default);
    }
}