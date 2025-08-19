using Microsoft.AspNetCore.Mvc;
using UserTasksApi.Models;
using UserTasksApi.Repositories;

namespace UserTasksApi.Controllers
{
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

        //Get all tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
        {
            var tasks = await _taskRepository.GetAllAsync();
            return Ok(tasks);
        }

        //Get task by Id
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTask(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return NotFound();
            return Ok(task);
        }

        //Create new task
        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
        {
            var user = await _userRepository.GetByIdAsync(task.AssigneeId);
            if (user == null) return BadRequest("Assignee not found.");

            var createdTask = await _taskRepository.AddAsync(task);
            return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
        }

        //Update task
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

        //Delete task
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return NotFound();

            await _taskRepository.DeleteAsync(task);
            return NoContent();
        }

       // Get all expired tasks
        [HttpGet("expired")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetExpiredTasks()
        {
            var allTasks = await _taskRepository.GetAllAsync();
            var expired = allTasks.Where(t => t.DueDate < DateTime.UtcNow).ToList();
            return Ok(expired);
        }

        // Get all active tasks 
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetActiveTasks()
        {
            var allTasks = await _taskRepository.GetAllAsync();
            var active = allTasks.Where(t => t.DueDate >= DateTime.UtcNow).ToList();
            return Ok(active);
        }

        // Get tasks by user
        [HttpGet("byuser/{userId}")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasksByUser(int userId)
        {
            var allTasks = await _taskRepository.GetAllAsync();
            var userTasks = allTasks.Where(t => t.AssigneeId == userId).ToList();
            return Ok(userTasks);
        }

        // Get tasks by date
        [HttpGet("bydate/{date}")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasksByDate(DateTime date)
        {
            var allTasks = await _taskRepository.GetAllAsync();
            var tasksOnDate = allTasks.Where(t => t.DueDate.Date == date.Date).ToList();
            return Ok(tasksOnDate);
        }
    }
}
