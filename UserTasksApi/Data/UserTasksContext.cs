using Microsoft.EntityFrameworkCore;
using UserTasksApi.Models;

namespace UserTasksApi.Data
{
    public class UserTasksContext : DbContext
    {
        public UserTasksContext(DbContextOptions<UserTasksContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
    }
}
