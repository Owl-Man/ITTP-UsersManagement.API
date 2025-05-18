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
    
    private User user, user2, admin;

    public UsersServiceTests()
    {
        _logger = new LoggerFactory().CreateLogger<UserService>();
        
        _userRepository = new Mock<IUserRepository>();
        _userService = new UserService(_logger, _userRepository.Object);
        
        SetupUserRepositoryWithUsers();
    }
    
    private void SetupUserRepositoryWithUsers()
    {
        user = CreateUser("user1", "user1", "user1", false);
        user2 = CreateUser("user2", "user2", "user2", false);
        admin = CreateUser("admin", "admin", "admin", true);
        
        _userRepository.Setup(r => r.GetByLogin("user1")).Returns(new RetrievedUserDto(user,string.Empty));
        _userRepository.Setup(r => r.GetByLogin("user2")).Returns(new RetrievedUserDto(user2,string.Empty));
        _userRepository.Setup(r => r.GetByLogin("admin")).Returns(new RetrievedUserDto(admin,string.Empty));
    }

    private User CreateUser(string login, string password, string name, bool admin)
    {
        return new UserEntity
        {
            Id = Guid.NewGuid(),
            Login = login,
            Password = password,
            Name = name,
            Gender = 1,
            Admin = admin,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "admin",
            ModifiedOn = DateTime.UtcNow,
            ModifiedBy = "admin",
            RevokedBy = "",
            RevokedOn = null,
        }.ToUser();
    }
    
    [Fact]
    public void UpdatePersonalInfo_RegularUserUpdatesOwnInfo_Success()
    {
        // Arrange
        _userRepository.Setup(r => r.UpdatePersonalInfo(user.Id, "NewName", 1, null, "user1"))
            .Returns(new RetrievedIdDto(user.Id, string.Empty));

        // Act
        RetrievedIdDto result = _userService.UpdatePersonalInfo("user1", "NewName", 1, null, "user1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Empty(result.Error);
    }

    [Fact]
    public void UpdatePersonalInfo_RegularUserUpdatesOtherUser_Fails()
    {
        // Arrange
        _userRepository.Setup(r => r.UpdatePersonalInfo(user.Id, "NewName", 1, null, "user1"))
            .Returns(new RetrievedIdDto(user.Id, string.Empty));
        
        // Act
        var result = _userService.UpdatePersonalInfo("user1", "NewName", 1, null, "user2");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Guid.Empty, result.Id);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public void UpdatePersonalInfo_AdminUpdatesUser_Success()
    {
        // Arrange
        _userRepository.Setup(r => r.UpdatePersonalInfo(user.Id, "NewName", 1, null, "admin"))
            .Returns(new RetrievedIdDto(user.Id, string.Empty));

        // Act
        var result = _userService.UpdatePersonalInfo("user1", "NewName", 1, null, "admin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Empty(result.Error);
    }

    [Fact]
    public void UpdatePersonalInfo_UserNotFound_Fails()
    {
        // Arrange
        _userRepository.Setup(r => r.GetByLogin("nonexistent")).Returns(new RetrievedUserDto(null, "User not found"));

        // Act
        var result = _userService.UpdatePersonalInfo("nonexistent", "NewName", 1, null, "admin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Guid.Empty, result.Id);
        Assert.Equal("User not found", result.Error);
    }

    [Fact]
    public void UpdateLogin_RegularUserUpdatesOwnLogin_Success()
    {
        // Arrange
        _userRepository.Setup(r => r.UpdateLogin(user.Id, "newlogin", "user1"))
            .Returns(new RetrievedIdDto(user.Id, string.Empty));

        // Act
        var result = _userService.UpdateLogin("user1", "newlogin", "user1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Empty(result.Error);
    }

    [Fact]
    public void UpdateLogin_AdminUpdatesUser_Success()
    {
        // Arrange
        _userRepository.Setup(r => r.UpdateLogin(user.Id, "newlogin", "admin"))
            .Returns(new RetrievedIdDto(user.Id, string.Empty));

        // Act
        var result = _userService.UpdateLogin("user1", "newlogin", "admin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Empty(result.Error);
    }

    [Fact]
    public void UpdateLogin_UserNotFound_Fails()
    {
        // Arrange
        _userRepository.Setup(r => r.GetByLogin("nonexistent")).Returns(new RetrievedUserDto(null, "User not found"));

        // Act
        var result = _userService.UpdateLogin("nonexistent", "newlogin", "admin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Guid.Empty, result.Id);
        Assert.Equal("User not found", result.Error);
    }
    
    [Fact]
    public void UpdatePassword_UserNotFound_Fails()
    {
        // Arrange
        _userRepository.Setup(r => r.GetByLogin("nonexistent")).Returns(new RetrievedUserDto(null, "User not found"));

        // Act
        var result = _userService.UpdatePassword("nonexistent", "newpassword", "admin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(Guid.Empty, result.Id);
        Assert.Equal("User not found", result.Error);
    }
}