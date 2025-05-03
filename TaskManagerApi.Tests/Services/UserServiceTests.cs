using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.DTOs;
using TaskManagerApi.Models;
using TaskManagerApi.Services;

namespace TaskManagerApi.Tests.Services;

public class UserServiceTests
{
    private static AppDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_CreatesUser_WhenNameIsUnique()
    {
        // Arrange
        var db = GetInMemoryContext();
        var service = new UserService(db);
        var dto = new CreateUserDto { Name = "UniqueUser" };

        // Act
        var user = await service.CreateAsync(dto);

        // Assert
        Assert.Equal(dto.Name, user.Name);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public async Task CreateAsync_ThrowsException_WhenNameIsDuplicate()
    {
        // Arrange
        var db = GetInMemoryContext();
        db.Users.Add(new User { Name = "DuplicateUser" });
        await db.SaveChangesAsync();

        var service = new UserService(db);
        var dto = new CreateUserDto { Name = "DuplicateUser" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ThrowsException_WhenNameIsEmpty()
    {
        // Arrange
        var db = GetInMemoryContext();
        var service = new UserService(db);
        var dto = new CreateUserDto { Name = "" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Arrange
        var db = GetInMemoryContext();
        db.Users.AddRange(
            new User { Name = "User1" },
            new User { Name = "User2" }
        );
        await db.SaveChangesAsync();

        var service = new UserService(db);

        // Act
        var users = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, users.Count);
    }
}