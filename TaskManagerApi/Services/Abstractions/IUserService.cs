using TaskManagerApi.DTOs;
using TaskManagerApi.Models;

namespace TaskManagerApi.Services.Abstractions;

public interface IUserService
{
    Task<User> CreateAsync(CreateUserDto dto);
    Task<List<User>> GetAllAsync();
}