using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.DTOs;
using TaskManagerApi.Enums;
using TaskManagerApi.Models;
using TaskManagerApi.Services.Abstractions;

namespace TaskManagerApi.Services;

public class TaskService(AppDbContext dbContext) : ITaskService
{
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
            task.AssignedUserId = chosenUserId;
            task.State = TaskState.InProgress;
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

        // Try to find users with 0 tasks
        var usersWithZeroTasks = allUsers
            .Where(u => !taskCountLookup.ContainsKey(u.Id))
            .ToList();

        if (usersWithZeroTasks.Count != 0)
        {
            // Pick random from users with zero tasks
            return usersWithZeroTasks.OrderBy(_ => Guid.NewGuid()).First().Id;
        }

        // Find the user(s) with the fewest tasks
        var minTaskCount = taskCountLookup.Min(x => x.Value);

        var usersWithMinTasks = allUsers
            .Where(u => taskCountLookup.TryGetValue(u.Id, out var count) && count == minTaskCount)
            .ToList();

        return usersWithMinTasks.OrderBy(_ => Guid.NewGuid()).First().Id;
    }

    #endregion
}