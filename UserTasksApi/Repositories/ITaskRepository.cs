using UserTasksApi.Models;

namespace UserTasksApi.Repositories
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskItem>> GetAllAsync();
        Task<TaskItem?> GetByIdAsync(int id);
        Task<TaskItem> AddAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(TaskItem task);
    }
}