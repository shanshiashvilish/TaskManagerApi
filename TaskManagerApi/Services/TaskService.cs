using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.DTOs;
using TaskManagerApi.Enums;
using TaskManagerApi.Models;
using TaskManagerApi.Services.Abstractions;

namespace TaskManagerApi.Services;

public class TaskService(AppDbContext dbContext) : ITaskService
{
    public Task ReassignTasksAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<TaskItem> CreateAsync(CreateTaskDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title is required!");

        var exists = await dbContext.Tasks.AnyAsync(t => t.Title == dto.Title);
        if (exists)
            throw new InvalidOperationException("Task with this title already exists!");

        var allUsers = await dbContext.Users.ToListAsync();
        var task = new TaskItem
        {
            Title = dto.Title,
            State = TaskState.Waiting
        };

        if (allUsers.Count != 0)
        {
            var chosenUserId = await GetUserIdForNewTaskAsync(allUsers);

            if (chosenUserId != Guid.Empty)
            {
                task.AssignedUserId = chosenUserId;
                task.State = TaskState.InProgress;
            }
        }

        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync();

        return task;
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await dbContext.Tasks
            .Include(t => t.AssignedUser)
            .ToListAsync();
    }


    #region Private Methods

    private async Task<Guid> GetUserIdForNewTaskAsync(List<User> allUsers)
    {
        // Group task count by user ID
        var taskCountsByUsers = await dbContext.Tasks
            .Where(t => t.AssignedUserId != null)
            .GroupBy(t => t.AssignedUserId)
            .Select(g => new
            {
                UserId = g.Key!.Value,
                Count = g.Count()
            })
            .ToListAsync();

        // Dictionary for quick access
        var taskCountLookup = taskCountsByUsers.ToDictionary(t => t.UserId, t => t.Count);

        // Find users with 0 tasks
        var usersWithZeroTasks = allUsers
            .Where(u => !taskCountLookup.ContainsKey(u.Id))
            .ToList();

        return usersWithZeroTasks.Count > 0
            ? usersWithZeroTasks.OrderBy(_ => Guid.NewGuid()).First().Id // Pick random from users with zero tasks
            : Guid.Empty;
    }

    #endregion
}