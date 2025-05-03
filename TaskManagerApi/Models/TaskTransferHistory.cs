namespace TaskManagerApi.Models;

public class TaskTransferHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public DateTime TransferredAt { get; set; } = DateTime.UtcNow;
}