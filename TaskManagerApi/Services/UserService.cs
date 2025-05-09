using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.DTOs;
using TaskManagerApi.Models;
using TaskManagerApi.Services.Abstractions;

namespace TaskManagerApi.Services;

public class UserService(AppDbContext dbContext, ILogger<UserService> logger) : IUserService
{
    public async Task<User> CreateAsync(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name is required!");

        var exists = await dbContext.Users.AnyAsync(u => u.Name == dto.Name);
        if (exists)
            throw new InvalidOperationException("User with this name already exists!");

        var user = new User { Name = dto.Name };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("User '{UserName}' created with ID: {UserId}", user.Name, user.Id);

        return user;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await dbContext.Users.Include(u => u.Tasks)
            .ToListAsync();
    }
}