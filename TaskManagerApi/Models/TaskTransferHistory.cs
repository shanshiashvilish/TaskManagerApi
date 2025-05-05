namespace TaskManagerApi.Models;

public class TaskTransferHistory
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime TransferredAt { get; set; } = DateTime.UtcNow;
}