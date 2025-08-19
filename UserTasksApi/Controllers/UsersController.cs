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

        // GET: api/tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
        {
            // Include Assignee details
            return await _context.Tasks.Include(t => t.Assignee).ToListAsync();
        }

        // GET: api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTask(int id)
        {
            var task = await _context.Tasks.Include(t => t.Assignee)
                                           .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();
            return task;
        }

        // POST: api/tasks
        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
        {
            // Optional: check if assigned user exists
            var user = await _context.Users.FindAsync(task.AssigneeId);
            if (user == null) return BadRequest("Assignee not found.");

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Return task with Assignee included
            await _context.Entry(task).Reference(t => t.Assignee).LoadAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        // PUT: api/tasks/{id}
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

        // DELETE: api/tasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
