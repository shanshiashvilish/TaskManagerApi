namespace TaskManagerApi.DTOs;

public class TaskTransferHistoryDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public DateTime TransferredAt { get; set; }
}