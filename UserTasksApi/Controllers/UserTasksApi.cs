using Microsoft.AspNetCore.Mvc;
using UserTasksApi.Models;

namespace UserTasksApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserTasksApi : ControllerBase
    {
        private static List<TaskItem> tasks = new List<TaskItem>();

        [HttpGet]
        public ActionResult<IEnumerable<TaskItem>> GetTasks() => Ok(tasks);

        [HttpGet("{id}")]
        public ActionResult<TaskItem> GetTask(int id)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpPost]
        public ActionResult<TaskItem> CreateTask(TaskItem task)
        {
            task.Id = tasks.Count + 1;
            tasks.Add(task);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTask(int id, TaskItem updatedTask)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task == null) return NotFound();

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.AssigneeId = updatedTask.AssigneeId;
            task.DueDate = updatedTask.DueDate;

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTask(int id)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task == null) return NotFound();

            tasks.Remove(task);
            return NoContent();
        }
    }
}