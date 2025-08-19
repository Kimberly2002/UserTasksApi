using UserTasksApi.Models;

namespace UserTasksApi.Repositories
{
    public interface IUserRepository
    {
            Task<IEnumerable<User>> GetAllAsync();
            Task<User?> GetByIdAsync(int id);
            Task<User> AddAsync(User user);
            Task UpdateAsync(User user);
            Task DeleteAsync(User user);
        }
    }
