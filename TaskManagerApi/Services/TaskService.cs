using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.DTOs;
using TaskManagerApi.Enums;
using TaskManagerApi.Models;
using TaskManagerApi.Services.Abstractions;

namespace TaskManagerApi.Services;

public class TaskService(AppDbContext dbContext, IUserService userService, ILogger<TaskService> logger) : ITaskService
{
    public async Task<TaskItem> CreateAsync(CreateTaskDto dto)
    {
        await ValidateTitle(dto.Title);

        var allUsers = await dbContext.Users.ToListAsync();

        var task = new TaskItem
        {
            Title = dto.Title,
            State = TaskState.Waiting
        };

        if (allUsers.Count > 0)
        {
            var chosenUserId = await GetUserIdForNewTaskAsync(allUsers);

            if (chosenUserId != Guid.Empty)
            {
                AssignTask(task, chosenUserId);
                logger.LogInformation("Task '{Title}' assigned to user {UserId} on creation.", dto.Title, chosenUserId);
            }
        }

        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Task '{Title}' created with ID: {TaskId}", dto.Title, task.Id);

        return task;
    }

    public async Task ReassignTasksAsync()
    {
        var tasks = (await GetAllAsync()).Where(t => t.State != TaskState.Completed);
        var allUsers = await userService.GetAllAsync();

        foreach (var task in tasks)
        {
            var history = await dbContext.TaskTransferHistories
                .Where(h => h.TaskId == task.Id)
                .OrderByDescending(h => h.TransferredAt)
                .ToListAsync();

            var currentUserId = task.AssignedUserId;
            var previousUserId = history.Skip(1).FirstOrDefault()?.UserId;

            var pastUserIds = history.Select(h => h.UserId).Distinct().ToList();
            var allUserIds = allUsers.Select(u => u.Id).ToList();

            var assignedToAll = allUserIds.All(id => pastUserIds.Contains(id));
            if (assignedToAll)
            {
                UnassignTask(task, TaskState.Completed);
                logger.LogInformation("Task '{TaskId}' marked as Completed. All users had it assigned.", task.Id);
                continue;
            }

            var eligibleUsers = allUsers
                .Where(u => u.Id != currentUserId && u.Id != previousUserId && !pastUserIds.Contains(u.Id))
                .ToList();

            if (eligibleUsers.Count == 0)
            {
                UnassignTask(task, TaskState.Waiting);
                logger.LogInformation("Task '{TaskId}' left unassigned due to no eligible users.", task.Id);
                continue;
            }

            var newUser = eligibleUsers.OrderBy(_ => Guid.NewGuid()).First();
            AssignTask(task, newUser.Id);
            logger.LogInformation("Task '{TaskId}' reassigned to user {UserId}.", task.Id, newUser.Id);
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        return await dbContext.Tasks
            .Include(t => t.AssignedUser)
            .ToListAsync();
    }

    public async Task<List<TaskTransferHistoryDto>> GetTaskHistoryAsync(Guid taskId)
    {
        var history = await dbContext.TaskTransferHistories
            .Where(h => h.TaskId == taskId)
            .OrderBy(h => h.TransferredAt)
            .Join(
                dbContext.Users,
                h => h.UserId,
                u => u.Id,
                (h, u) => new TaskTransferHistoryDto()
                {
                    UserId = u.Id,
                    UserName = u.Name,
                    TransferredAt = h.TransferredAt
                }
            )
            .ToListAsync();

        return history;
    }

    #region Private Methods

    private async Task ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required!");

        var exists = await dbContext.Tasks.AnyAsync(t => t.Title == title);
        if (exists)
            throw new InvalidOperationException("Task with this title already exists!");
    }

    private void AssignTask(TaskItem task, Guid userId)
    {
        task.AssignedUserId = userId;
        task.State = TaskState.InProgress;

        dbContext.TaskTransferHistories.Add(new TaskTransferHistory
        {
            TaskId = task.Id,
            UserId = userId,
            TransferredAt = DateTime.UtcNow
        });
    }

    private static void UnassignTask(TaskItem task, TaskState state)
    {
        task.AssignedUserId = null;
        task.State = state;

        if (state == TaskState.Completed)
            task.CompletedAt = DateTime.UtcNow;
    }

    private async Task<Guid> GetUserIdForNewTaskAsync(List<User> allUsers)
    {
        var taskCountsByUsers = await dbContext.Tasks
            .Where(t => t.AssignedUserId != null)
            .GroupBy(t => t.AssignedUserId)
            .Select(g => new
            {
                UserId = g.Key!.Value,
                Count = g.Count()
            })
            .ToListAsync();

        var taskCountLookup = taskCountsByUsers.ToDictionary(t => t.UserId, t => t.Count);

        var usersWithZeroTasks = allUsers
            .Where(u => !taskCountLookup.ContainsKey(u.Id))
            .ToList();

        return usersWithZeroTasks.Count > 0
            ? usersWithZeroTasks.OrderBy(_ => Guid.NewGuid()).First().Id
            : Guid.Empty;
    }

    #endregion
}