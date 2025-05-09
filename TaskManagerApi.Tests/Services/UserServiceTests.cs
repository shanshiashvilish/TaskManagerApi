using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagerApi.Data;
using TaskManagerApi.DTOs;
using TaskManagerApi.Models;
using TaskManagerApi.Services;

namespace TaskManagerApi.Tests.Services;

public class UserServiceTests
{
    private readonly AppDbContext _context;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"UserTestDb-{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);
        var mockLogger = new Mock<ILogger<UserService>>();
        _userService = new UserService(_context, mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateUser_WhenNameIsUnique()
    {
        // Arrange
        var dto = new CreateUserDto { Name = "UniqueUser" };

        // Act
        var user = await _userService.CreateAsync(dto);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(dto.Name, user.Name);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenNameAlreadyExists()
    {
        // Arrange
        const string name = "DuplicateUser";
        _context.Users.Add(new User { Name = name });
        await _context.SaveChangesAsync();

        var dto = new CreateUserDto { Name = name };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateAsync(dto));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        _context.Users.AddRange(
            new User { Name = "User1" },
            new User { Name = "User2" }
        );
        await _context.SaveChangesAsync();

        // Act
        var users = await _userService.GetAllAsync();

        // Assert
        Assert.NotNull(users);
        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        // Act
        var users = await _userService.GetAllAsync();

        // Assert
        Assert.NotNull(users);
        Assert.Empty(users);
    }
}