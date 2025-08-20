using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserTasksApi.Models;
using UserTasksApi.Repositories;

namespace UserTasksApi.Controllers
{
    /// <summary>
    /// Controller for managing tasks.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;

        public TasksController(ITaskRepository taskRepository, IUserRepository userRepository)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get all tasks.
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks() => Ok(await _taskRepository.GetAllAsync());

        /// <summary>
        /// Get a specific task by ID.
        /// </summary>
        /// <param name="id">Task ID</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTask(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return NotFound();
            return Ok(task);
        }

        /// <summary>
        /// Create a new task.
        /// </summary>
        /// <param name="task">Task details</param>
        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
        {
            var user = await _userRepository.GetByIdAsync(task.AssigneeId);
            if (user == null) return BadRequest("Assignee not found.");

            var createdTask = await _taskRepository.AddAsync(task);
            return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
        }

        /// <summary>
        /// Update an existing task.
        /// </summary>
        /// <param name="id">Task ID</param>
        /// <param name="updatedTask">Updated task details</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem updatedTask)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return NotFound();

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.AssigneeId = updatedTask.AssigneeId;
            task.DueDate = updatedTask.DueDate;

            await _taskRepository.UpdateAsync(task);
            return NoContent();
        }

        /// <summary>
        /// Delete a task by ID.
        /// </summary>
        /// <param name="id">Task ID</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return NotFound();

            await _taskRepository.DeleteAsync(task);
            return NoContent();
        }

        /// <summary>
        /// Get all expired tasks (past due date).
        /// </summary>
        [HttpGet("expired")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetExpiredTasks()
        {
            var allTasks = await _taskRepository.GetAllAsync();
            return Ok(allTasks.Where(t => t.DueDate < DateTime.UtcNow));
        }

        /// <summary>
        /// Get all active tasks (not yet due).
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetActiveTasks()
        {
            var allTasks = await _taskRepository.GetAllAsync();
            return Ok(allTasks.Where(t => t.DueDate >= DateTime.UtcNow));
        }

        /// <summary>
        /// Get tasks assigned to a specific user.
        /// </summary>
        /// <param name="userId">User ID</param>
        [HttpGet("byuser/{userId}")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasksByUser(int userId)
        {
            var allTasks = await _taskRepository.GetAllAsync();
            return Ok(allTasks.Where(t => t.AssigneeId == userId));
        }

        /// <summary>
        /// Get tasks due on a specific date.
        /// </summary>
        /// <param name="date">Target date (yyyy-MM-dd)</param>
        [HttpGet("bydate/{date}")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasksByDate(DateTime date)
        {
            var allTasks = await _taskRepository.GetAllAsync();
            return Ok(allTasks.Where(t => t.DueDate.Date == date.Date));
        }
    }
}
