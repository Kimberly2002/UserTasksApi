using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserTasksApi.Data;
using UserTasksApi.Models;

namespace UserTasksApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly UserTasksContext _context;

        public TasksController(UserTasksContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
        {
            return await _context.Tasks.Include(t => t.Assignee).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTask(int id)
        {
            var task = await _context.Tasks.Include(t => t.Assignee)
                                           .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();
            return task;
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
        {
            var user = await _context.Users.FindAsync(task.AssigneeId);
            if (user == null) return BadRequest("Assignee not found.");

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            await _context.Entry(task).Reference(t => t.Assignee).LoadAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem updatedTask)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.AssigneeId = updatedTask.AssigneeId;
            task.DueDate = updatedTask.DueDate;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpGet("expired")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetExpiredTasks()
        {
            var now = DateTime.UtcNow;
            return await _context.Tasks
                .Where(t => t.DueDate < now)
                .Include(t => t.Assignee)
                .ToListAsync();
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetActiveTasks()
        {
            var now = DateTime.UtcNow;
            return await _context.Tasks
                .Where(t => t.DueDate >= now)
                .Include(t => t.Assignee)
                .ToListAsync();
        }

        [HttpGet("due/{date}")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasksByDueDate(DateTime date)
        {
            return await _context.Tasks
                .Where(t => t.DueDate.Date == date.Date)
                .Include(t => t.Assignee)
                .ToListAsync();
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasksByUser(int userId)
        {
            return await _context.Tasks
                .Where(t => t.AssigneeId == userId)
                .Include(t => t.Assignee)
                .ToListAsync();
        }


        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> SearchTasks(
            string? keyword,
            string? sortBy = "duedate",
            string? order = "asc")
        {
            var query = _context.Tasks.Include(t => t.Assignee).AsQueryable();

            // Search by title or description
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t =>
                    t.Title.Contains(keyword) ||
                    t.Description.Contains(keyword));
            }

            // Sorting logic
            query = (sortBy?.ToLower(), order?.ToLower()) switch
            {
                ("title", "asc") => query.OrderBy(t => t.Title),
                ("title", "desc") => query.OrderByDescending(t => t.Title),
                ("duedate", "asc") => query.OrderBy(t => t.DueDate),
                ("duedate", "desc") => query.OrderByDescending(t => t.DueDate),
                ("assignee", "asc") => query.OrderBy(t => t.Assignee.Username),
                ("assignee", "desc") => query.OrderByDescending(t => t.Assignee.Username),
                _ => query.OrderBy(t => t.DueDate) 
            };

            return await query.ToListAsync();
        }
    }
}
