using UserTasksApi.Models;

namespace UserTasksApi.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByEmailAndPasswordAsync(string email, string password); 
        Task<User> AddUserAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
    }
}
