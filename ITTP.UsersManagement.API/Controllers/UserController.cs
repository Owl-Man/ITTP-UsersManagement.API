using ITTP.UsersManagement.API.Application.Services;
using ITTP.UsersManagement.API.Core;
using ITTP.UsersManagement.API.Core.DTOs;
using ITTP.UsersManagement.API.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserManagementApi.Controllers
{
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

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDto userLoginDto)
        {
            var token = _authService.Authenticate(userLoginDto);
            return Ok(new { Token = token });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateUser([FromBody] CreateUserDto dto)
        {
            string? currentUser = User.Identity.Name;
            
            RetrievedId retrievedId = _userService.CreateUser(dto.Login, dto.Password, dto.Name, dto.Gender, dto.Birthday, dto.Admin, currentUser);

            if (!string.IsNullOrEmpty(retrievedId.error)) return BadRequest(retrievedId.error);
            
            return Ok(new { login = dto.Login });
        }

        [HttpPut("{login}")]
        [Authorize]
        public IActionResult UpdateUser(string login, [FromBody] UserPersonalInfoDto personalInfoDto)
        {
            string? currentUser = User.Identity.Name;
            
            RetrievedId retrievedId = _userService.UpdatePersonalInfo(login, personalInfoDto.Name, personalInfoDto.Gender, personalInfoDto.Birthday, currentUser);
            
            if (!string.IsNullOrEmpty(retrievedId.error)) return BadRequest(retrievedId.error);
            
            return NoContent();
        }

        [HttpPut("{login}/password")]
        [Authorize]
        public IActionResult UpdatePassword(string login, [FromBody] UpdatePasswordDto dto)
        {
            var currentUser = User.Identity.Name;
            
            RetrievedId retrievedId = _userService.UpdatePassword(login, dto.Password, currentUser);
            if (!string.IsNullOrEmpty(retrievedId.error)) return BadRequest(retrievedId.error);
            
            return NoContent();
        }

        [HttpPut("{login}/login")]
        [Authorize]
        public IActionResult UpdateLogin(string login, [FromBody] UpdateLoginDto dto)
        {
            string? currentUser = User.Identity.Name;
            
            RetrievedId retrievedId = _userService.UpdateLogin(login, dto.Login, currentUser);
            if (!string.IsNullOrEmpty(retrievedId.error)) return BadRequest(retrievedId.error);
            
            return NoContent();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetActiveUsers()
        {
            List<User> users = _userService.GetActiveUsers();
            
            return Ok(users);
        }

        [HttpGet("{login}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetUserByLogin(string login)
        {
            string? currentUser = User.Identity.Name;
            
            RetrievedUser retrievedUser = _userService.GetUserByLogin(login, currentUser);
            
            if (!string.IsNullOrEmpty(retrievedUser.error)) return BadRequest(retrievedUser.error);

            UserPersonalInfoDto userPersonalInfoDto = retrievedUser.user.ToUserPersonalInfoDto();
            
            return Ok(userPersonalInfoDto);
        }

        [HttpGet("{login}/getByLoginAndPassword")]
        [Authorize(Roles = "User")]
        public IActionResult GetUserByLoginAndPassword([FromBody] UserLoginDto userLoginDto)
        {
            string? currentUser = User.Identity.Name;
            
            RetrievedUser retrievedUser = _userService.GetByLoginAndPassword(userLoginDto.Login, userLoginDto.Password, currentUser);
            
            if (!string.IsNullOrEmpty(retrievedUser.error)) return BadRequest(retrievedUser.error);
            
            return Ok(retrievedUser);
        }

        [HttpGet("age/{age}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetUsersOlderThan(int age)
        {
            var users = _userService.GetUsersOlderThan(age);
            return Ok(users);
        }

        [HttpDelete("{login}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteUser(string login, bool softDelete)
        {
            string? currentUser = User.Identity.Name;

            RetrievedId retrievedId;
            
            retrievedId = softDelete ? _userService.DeleteUser(login, currentUser) : _userService.DeleteUserForce(login);
            
            if (!string.IsNullOrEmpty(retrievedId.error)) return BadRequest(retrievedId.error);
            
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
}