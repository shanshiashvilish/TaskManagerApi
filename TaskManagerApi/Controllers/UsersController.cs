using Microsoft.AspNetCore.Mvc;
using TaskManagerApi.DTOs;
using TaskManagerApi.Services.Abstractions;

namespace TaskManagerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUserDto dto)
    {
        try
        {
            var user = await userService.CreateAsync(dto);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var users = await userService.GetAllAsync();
        return Ok(users);
    }
}