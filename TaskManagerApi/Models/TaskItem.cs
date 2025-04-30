using TaskManagerApi.Enums;

namespace TaskManagerApi.Models;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public TaskState State { get; set; } = TaskState.Waiting;

    public Guid? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}