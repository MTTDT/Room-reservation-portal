using Backend.DTOs.Users;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(IUserService userService) : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAllUsers(CancellationToken ct)
        {
            var users = await userService.GetAllUsersAsync(ct);
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponse>> GetUserById(int id, CancellationToken ct)
        {
            var user = await userService.GetUserByIdAsync(id, ct);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await userService.CreateUserAsync(request, ct);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<UserResponse>> UpdateUser(int id, [FromBody] UpdateUserRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await userService.UpdateUserAsync(id, request, ct);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteUser(int id, CancellationToken ct)
        {
            await userService.DeleteUserAsync(id, ct);
            return NoContent(); 
        }
    }
}