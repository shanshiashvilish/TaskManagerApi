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
    private readonly AppDbContext _context;
    private readonly TaskService _taskService;
    private readonly Mock<IUserService> _mockUserService;

    public TaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TaskTestDb-{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);
        _mockUserService = new Mock<IUserService>();
        var mockLogger = new Mock<ILogger<TaskService>>();

        _taskService = new TaskService(_context, _mockUserService.Object, mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTask_WhenTitleIsUnique()
    {
        // Arrange
        var dto = new CreateTaskDto { Title = "New Task" };
        var user = new User { Id = Guid.NewGuid(), Name = "TestUser" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _mockUserService.Setup(s => s.GetAllAsync()).ReturnsAsync([user]);

        // Act
        var task = await _taskService.CreateAsync(dto);

        // Assert
        Assert.NotNull(task);
        Assert.Equal(TaskState.InProgress, task.State);
        Assert.Equal(dto.Title, task.Title);
        Assert.NotNull(task.AssignedUserId);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTaskInWaitingState_WhenNoUsersExist()
    {
        // Arrange
        var dto = new CreateTaskDto { Title = "Waiting Task" };
        _mockUserService.Setup(s => s.GetAllAsync()).ReturnsAsync([]);

        // Act
        var task = await _taskService.CreateAsync(dto);

        // Assert
        Assert.NotNull(task);
        Assert.Equal(TaskState.Waiting, task.State);
        Assert.Null(task.AssignedUserId);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenTitleAlreadyExists()
    {
        // Arrange
        const string title = "Duplicate Task";
        _context.Tasks.Add(new TaskItem { Title = title });
        await _context.SaveChangesAsync();
        var dto = new CreateTaskDto { Title = title };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _taskService.CreateAsync(dto));
    }

    [Fact]
    public async Task ReassignTasksAsync_ShouldCompleteTasks_WhenAllUsersAssigned()
    {
        // Arrange
        var user1 = new User { Name = "User1" };
        var user2 = new User { Name = "User2" };
        await _context.Users.AddRangeAsync(user1, user2);

        var task = new TaskItem { Title = "Task1", AssignedUserId = user1.Id, State = TaskState.InProgress };
        await _context.Tasks.AddAsync(task);

        await _context.TaskTransferHistories.AddRangeAsync(
            new TaskTransferHistory { TaskId = task.Id, UserId = user1.Id },
            new TaskTransferHistory { TaskId = task.Id, UserId = user2.Id }
        );
        await _context.SaveChangesAsync();

        _mockUserService.Setup(s => s.GetAllAsync()).ReturnsAsync([user1, user2]);

        // Act
        await _taskService.ReassignTasksAsync();

        // Assert
        var updatedTask = await _context.Tasks.FindAsync(task.Id);
        Assert.Equal(TaskState.Completed, updatedTask.State);
        Assert.Null(updatedTask.AssignedUserId);
    }

    [Fact]
    public async Task ReassignTasksAsync_ShouldAssignNewUser_WhenOnlyOneUserPreviouslyAssigned()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid(), Name = "User1" };
        var user2 = new User { Id = Guid.NewGuid(), Name = "User2" };
        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        var task = new TaskItem { Title = "Task2", AssignedUserId = user1.Id, State = TaskState.InProgress };
        await _context.Tasks.AddAsync(task);

        await _context.TaskTransferHistories.AddAsync(new TaskTransferHistory { TaskId = task.Id, UserId = user1.Id });
        await _context.SaveChangesAsync();

        _mockUserService.Setup(s => s.GetAllAsync()).ReturnsAsync([user1, user2]);

        // Act
        await _taskService.ReassignTasksAsync();

        // Assert
        var updatedTask = await _context.Tasks.FindAsync(task.Id);
        Assert.Equal(TaskState.InProgress, updatedTask.State);
        Assert.Equal(user2.Id, updatedTask.AssignedUserId);
    }
}
