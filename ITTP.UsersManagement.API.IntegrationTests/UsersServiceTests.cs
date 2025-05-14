using ITTP.UsersManagement.API.Application;
using ITTP.UsersManagement.API.Application.Services;
using ITTP.UsersManagement.API.Core.DTOs;
using ITTP.UsersManagement.API.Core.Interfaces;
using ITTP.UsersManagement.API.Core.Models;
using ITTP.UsersManagement.API.DataAccess;
using ITTP.UsersManagement.API.DataAccess.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace ITTP.UsersManagement.API.IntegrationTests;

public class UsersServiceTests
{
    private readonly ILogger<UserService> _logger;
    private readonly Mock<IUserRepository> _userRepository;
    private readonly UserService _userService;

    public UsersServiceTests()
    {
        _logger = new LoggerFactory().CreateLogger<UserService>();
        
        _userRepository = new Mock<IUserRepository>();
        _userService = new UserService(_logger, _userRepository.Object);
    }
    
    [Fact]
    public void CreateUser_Success()
    {
        User user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Login = "testuser",
            Password = "password",
            Name = "Test User",
            Gender = 1,
            Admin = false,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "admin",
            ModifiedOn = DateTime.UtcNow,
            ModifiedBy = "admin"
        }.ToUser();
        
        //Arrange
        
        _userRepository.Setup<RetrievedId>( r => 
                r.Create("testuser", "password", "Test User", 1, null, false, "admin"))
            .Returns(new RetrievedId(user.Id, string.Empty));
        
        _userRepository.Setup(r => r.GetByLogin("testuser")).Returns(new RetrievedUser(user, string.Empty));
        
        //Act
        
        (Guid userId, string error) = _userService.CreateUser(user.Login, user.Password, user.Name, user.Gender, user.Birthday, user.Admin, user.CreatedBy);
        (User? retrievedUser, string errorR) = _userService.GetUserByLogin("testuser", user.Login);
        
        //Assert
        
        Assert.NotNull(retrievedUser);
        Assert.Equal(user.Id, retrievedUser.Id);
        Assert.Equal(user.Login, retrievedUser.Login);
        Assert.Equal(user.Name, retrievedUser.Name);
    }

    [Fact]
    public void UpdateUser_UpdatesCorrectly()
    {
        // Arrange
        
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Login = "testuser",
            Password = "password",
            Name = "Test User",
            Gender = 1,
            Admin = false,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "admin",
            ModifiedOn = DateTime.UtcNow,
            ModifiedBy = "admin"
        }.ToUser();
        _userRepository.Setup(r => r.GetByLogin("testuser")).Returns(new RetrievedUser(user,  string.Empty));
        
        // Act
        
        _userService.UpdatePersonalInfo("testuser", "testuser", 2, null, "admin");
        (User? retrievedUser, string error) = _userService.GetUserByLogin("testuser", user.Login);

        // Assert
        
        Assert.Equal("Test User", retrievedUser.Name);
        Assert.Equal(1, retrievedUser.Gender);
    }
}