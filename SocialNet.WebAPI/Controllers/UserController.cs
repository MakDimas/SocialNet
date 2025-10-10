using Microsoft.AspNetCore.Mvc;
using SocialNet.Core.Dtos.Models;
using SocialNet.Core.Dtos.UserDtos;
using SocialNet.Core.Services.Users;

namespace SocialNet.WebAPI.Controllers;

public class UserController : BaseController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
    {
        await _userService.CreateUserAsync(userDto);

        return Ok(new ActionResultWrapper<object>
        {
            Status = StatusCodes.Status200OK,
            Data = null,
            Message = "User was created successfully"
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetUserByFullNameAndId([FromQuery] UserInfoFromUrl userInfo)
    {
        return Ok(new ActionResultWrapper<UserResponseDto>
        {
            Status = StatusCodes.Status200OK,
            Data = await _userService.GetUserByFullNameAndIdAsync(userInfo),
            Message = "User retrieved"
        });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updateDto)
    {
        return Ok(new ActionResultWrapper<UserResponseDto>
        {
            Status = StatusCodes.Status200OK,
            Data = await _userService.UpdateUserAsync(updateDto),
            Message = "User updated successfully"
        });
    }
} 
