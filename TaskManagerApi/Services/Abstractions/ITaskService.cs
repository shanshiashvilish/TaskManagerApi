using TaskManagerApi.DTOs;
using TaskManagerApi.Models;

namespace TaskManagerApi.Services.Abstractions;

public interface ITaskService
{
    Task ReassignTasksAsync();
    Task<TaskItem> CreateAsync(CreateTaskDto dto);
    Task<List<TaskItem>> GetAllAsync();
    Task<List<TaskTransferHistoryDto>> GetTaskHistoryAsync(Guid taskId);
}