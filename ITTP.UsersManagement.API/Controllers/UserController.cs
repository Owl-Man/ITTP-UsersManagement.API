using ITTP.UsersManagement.API.Application.Services;
using ITTP.UsersManagement.API.Core;
using ITTP.UsersManagement.API.Core.DTOs;
using ITTP.UsersManagement.API.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITTP.UsersManagement.API.Controllers;

[Route("[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly AuthService _authService;

    public UsersController(UserService userService, AuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    [HttpPost("auth/login")]
    public IActionResult Login([FromBody] LoginAndPasswordDto loginAndPasswordDto)
    {
        (string token, string error) = _authService.Authenticate(loginAndPasswordDto);

        if (!string.IsNullOrEmpty(error))
        {
            return Unauthorized(new { Error = error });
        }
            
        return Ok(new LoginTokenDto(token));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateUser([FromBody] CreateUserDto createUserDto)
    {
        string currentUser = User.Identity.Name;
            
        RetrievedIdDto retrievedIdDto = _userService.CreateUser(createUserDto.Login, createUserDto.Password, 
            createUserDto.Name, createUserDto.Gender, createUserDto.Birthday, createUserDto.Admin, currentUser);

        if (!string.IsNullOrEmpty(retrievedIdDto.Error)) return BadRequest(retrievedIdDto.Error);
            
        return Ok(new LoginDto(createUserDto.Login));
    }

    [HttpPut("{login}/updateUser")]
    [Authorize]
    public IActionResult UpdateUser(string login, [FromBody] UserPersonalInfoDto personalInfoDto)
    {
        string currentUser = User.Identity.Name;
            
        RetrievedIdDto retrievedIdDto = _userService.UpdatePersonalInfo(login, personalInfoDto.Name, personalInfoDto.Gender, personalInfoDto.Birthday, currentUser);
            
        if (!string.IsNullOrEmpty(retrievedIdDto.Error)) return BadRequest(retrievedIdDto.Error);
            
        return NoContent();
    }

    [HttpPut("{login}/updatePassword")]
    [Authorize]
    public IActionResult UpdatePassword(string login, [FromBody] UpdatePasswordDto dto)
    {
        string currentUser = User.Identity.Name;
            
        RetrievedIdDto retrievedIdDto = _userService.UpdatePassword(login, dto.Password, currentUser);
        if (!string.IsNullOrEmpty(retrievedIdDto.Error)) return BadRequest(retrievedIdDto.Error);
            
        return NoContent();
    }

    [HttpPut("{login}/updateLogin")]
    [Authorize]
    public IActionResult UpdateLogin(string login, [FromBody] LoginDto dto)
    {
        string currentUser = User.Identity.Name;
            
        RetrievedIdDto retrievedIdDto = _userService.UpdateLogin(login, dto.Login, currentUser);
        if (!string.IsNullOrEmpty(retrievedIdDto.Error)) return BadRequest(retrievedIdDto.Error);
            
        return NoContent();
    }

    [HttpGet("getActiveUsers")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetActiveUsers()
    {
        List<User> users = _userService.GetActiveUsers();
            
        return Ok(users);
    }

    [HttpGet("getUserByLogin")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetUserByLogin(string login)
    {
        RetrievedUserDto retrievedUserDto = _userService.GetUserByLogin(login);
            
        if (!string.IsNullOrEmpty(retrievedUserDto.error)) return BadRequest(retrievedUserDto.error);

        UserPersonalInfoDto userPersonalInfoDto = retrievedUserDto.user.ToUserPersonalInfo();
            
        return Ok(userPersonalInfoDto);
    }

    [HttpGet("{login}")]
    [Authorize(Roles = "User")]
    public IActionResult GetUserByLoginAndPassword(string login, [FromQuery] string password)
    {
        string currentUser = User.Identity.Name;
            
        RetrievedUserDto retrievedUserDto = _userService.GetByLoginAndPassword(login, password, currentUser);
            
        if (!string.IsNullOrEmpty(retrievedUserDto.error)) return BadRequest(retrievedUserDto.error);
            
        return Ok(retrievedUserDto);
    }

    [HttpGet("getUsersOlderThan")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetUsersOlderThan(int age)
    {
        List<User> users = _userService.GetUsersOlderThan(age);
        return Ok(users);
    }

    [HttpDelete("{login}")]
    [Authorize(Roles = "Admin")]
    public IActionResult DeleteUser(string login, [FromQuery] bool softDelete)
    {
        string currentUser = User.Identity.Name;

        RetrievedIdDto retrievedIdDto;
            
        retrievedIdDto = softDelete ? _userService.DeleteUser(login, currentUser) : _userService.DeleteUserForce(login);
            
        if (!string.IsNullOrEmpty(retrievedIdDto.Error)) return BadRequest(retrievedIdDto.Error);
            
        return NoContent();
    }

    [HttpPut("{login}/restore")]
    [Authorize(Roles = "Admin")]
    public IActionResult RestoreUser(string login)
    {
        _userService.RestoreUser(login);
        return NoContent();
    }
}