namespace UserTasksApi.Models
{
    public class TaskItem
    {
        // Primary Key
        public int Id { get; set; }   
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Foreign Key to User
        public int AssigneeId { get; set; }
        public User? Assignee { get; set; }

        public DateTime DueDate { get; set; }
    }
}
