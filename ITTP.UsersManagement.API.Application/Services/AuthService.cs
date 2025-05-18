using System.Security.Claims;
using System.Text;
using ITTP.UsersManagement.API.Core;
using ITTP.UsersManagement.API.Core.DTOs;
using ITTP.UsersManagement.API.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ITTP.UsersManagement.API.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    private readonly TimeSpan expiresTime;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        
        expiresTime = TimeSpan.FromMinutes(_configuration.GetValue<int>("JwtSettings:ExpirationInMinutes"));
    }

    public (string token, string error) Authenticate(LoginAndPasswordDto loginAndPasswordDto)
    {

        try
        {
            RetrievedUserDto retrievedUserDto = _userRepository.GetByLogin(loginAndPasswordDto.Login);
        
            if (!string.IsNullOrEmpty(retrievedUserDto.error))
                return (string.Empty, retrievedUserDto.error);

            if (retrievedUserDto.user.RevokedOn != null || !BCrypt.Net.BCrypt.Verify(loginAndPasswordDto.Password, retrievedUserDto.user.Password))
                return (string.Empty, ErrorForm.AuthenticateError());

            JsonWebTokenHandler tokenHandler = new JsonWebTokenHandler();
        
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
        
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, retrievedUserDto.user.Login),
                    new Claim(ClaimTypes.Role, retrievedUserDto.user.Admin ? "Admin" : "User")
                }),
                Expires = DateTime.UtcNow.Add(expiresTime),
                SigningCredentials = credentials,
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"]
            };

            string token = tokenHandler.CreateToken(tokenDescriptor);
        
            return (token, string.Empty);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return (string.Empty, e.Message);
        }
    }
}