using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ITTP.UsersManagement.API.Core.DTOs;
using ITTP.UsersManagement.API.Core.Interfaces;
using ITTP.UsersManagement.API.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ITTP.UsersManagement.API.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public string Authenticate(UserLoginDto userLoginDto)
    {
        (User? user, string error) = _userRepository.GetByLoginAndPassword(userLoginDto.Login, userLoginDto.Password);

        if (user == null || user.RevokedOn != null)
            throw new UnauthorizedAccessException("Invalid credentials or user is revoked");

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"]);
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.Admin ? "Admin" : "User")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}