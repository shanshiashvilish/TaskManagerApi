using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagerApi.Data;
using TaskManagerApi.DTOs;
using TaskManagerApi.Enums;
using TaskManagerApi.Models;
using TaskManagerApi.Services;
using TaskManagerApi.Services.Abstractions;

namespace TaskManagerApi.Tests.Services;

public class TaskServiceTests
{
    private static ILogger<T> GetMockLogger<T>() => new Mock<ILogger<T>>().Object;

    private static AppDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static IUserService GetMockUserService(List<User> users)
    {
        var mock = new Mock<IUserService>();
        mock.Setup(s => s.GetAllAsync()).ReturnsAsync(users);
        return mock.Object;
    }

    [Fact]
    public async Task CreateAsync_AssignsTaskToUserWithZeroTasks()
    {
        // Arrange
        var db = GetInMemoryContext();
        var user = new User { Id = Guid.NewGuid(), Name = "UserA" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var logger = GetMockLogger<TaskService>();
        var service = new TaskService(db, GetMockUserService([user]), logger);

        // Act
        var task = await service.CreateAsync(new CreateTaskDto { Title = "Task 1" });

        // Assert
        Assert.Equal(TaskState.InProgress, task.State);
        Assert.Equal(user.Id, task.AssignedUserId);
    }

    [Fact]
    public async Task CreateAsync_TaskStaysWaiting_WhenNoUsersExist()
    {
        var db = GetInMemoryContext();
        var logger = GetMockLogger<TaskService>();
        var service = new TaskService(db, GetMockUserService([]), logger);

        var task = await service.CreateAsync(new CreateTaskDto { Title = "Task 2" });

        Assert.Equal(TaskState.Waiting, task.State);
        Assert.Null(task.AssignedUserId);
    }

    [Fact]
    public async Task ReassignTasksAsync_DoesNotReassignToSameOrPreviousUser()
    {
        // Arrange
        var db = GetInMemoryContext();
        var user1 = new User { Id = Guid.NewGuid(), Name = "User1" };
        var user2 = new User { Id = Guid.NewGuid(), Name = "User2" };
        var user3 = new User { Id = Guid.NewGuid(), Name = "User3" };
        db.Users.AddRange(user1, user2, user3);

        var task = new TaskItem
            { Id = Guid.NewGuid(), Title = "TestTask", AssignedUserId = user1.Id, State = TaskState.InProgress };
        db.Tasks.Add(task);

        db.TaskTransferHistories.AddRange(
            new TaskTransferHistory
                { TaskId = task.Id, UserId = user1.Id, TransferredAt = DateTime.UtcNow.AddMinutes(-4) },
            new TaskTransferHistory
                { TaskId = task.Id, UserId = user2.Id, TransferredAt = DateTime.UtcNow.AddMinutes(-2) }
        );

        await db.SaveChangesAsync();
        var logger = GetMockLogger<TaskService>();
        var service = new TaskService(db, GetMockUserService([user1, user2, user3]), logger);

        // Act
        await service.ReassignTasksAsync();

        // Assert
        var updatedTask = await db.Tasks.FindAsync(task.Id);
        Assert.NotEqual(user1.Id, updatedTask!.AssignedUserId);
        Assert.NotEqual(user2.Id, updatedTask.AssignedUserId);
        Assert.Equal(TaskState.InProgress, updatedTask.State);
    }
}