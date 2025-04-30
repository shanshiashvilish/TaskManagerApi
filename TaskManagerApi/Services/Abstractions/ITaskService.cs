using TaskManagerApi.DTOs;
using TaskManagerApi.Models;

namespace TaskManagerApi.Services.Abstractions;

public interface ITaskService
{
    Task<TaskItem> CreateAsync(CreateTaskDto dto);
    Task<List<TaskItem>> GetAllAsync();
}