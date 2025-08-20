using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserTasksApi.Models;
using UserTasksApi.Repositories;
using BCrypt.Net;

namespace UserTasksApi.Controllers
{
    /// <summary>
    /// Controller for managing users.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITaskRepository _taskRepository;

        public UsersController(IUserRepository userRepository, ITaskRepository taskRepository)
        {
            _userRepository = userRepository;
            _taskRepository = taskRepository;
        }

        /// <summary>
        /// Get all users.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers() =>
            Ok(await _userRepository.GetAllAsync());

        /// <summary>
        /// Get a specific user by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        /// <summary>
        /// Create a new user (hashes password).
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(UserDto userDto)
        {
            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password)
            };

            var createdUser = await _userRepository.AddUserAsync(user);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
        }

        /// <summary>
        /// Update an existing user.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserDto userDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound();

            user.Username = userDto.Username;
            user.Email = userDto.Email;

            if (!string.IsNullOrEmpty(userDto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            }

            await _userRepository.UpdateAsync(user);
            return NoContent();
        }

        /// <summary>
        /// Delete a user by ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != id.ToString())
                return Forbid("You can only delete your own account.");

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound();

            await _userRepository.DeleteAsync(user);
            return NoContent();
        }

        /// <summary>
        /// Get all tasks assigned to a specific user.
        /// </summary>
        [HttpGet("{id}/tasks")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetUserTasks(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != id.ToString())
                return Forbid("You can only access your own tasks.");

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound();

            var allTasks = await _taskRepository.GetAllAsync();
            return Ok(allTasks.Where(t => t.AssigneeId == id));
        }
    }
}
