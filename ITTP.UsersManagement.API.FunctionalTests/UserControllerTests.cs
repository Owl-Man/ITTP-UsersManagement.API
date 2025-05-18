using System.Net;
using System.Net.Http.Json;
using ITTP.UsersManagement.API.Controllers;
using ITTP.UsersManagement.API.Core.DTOs;
using ITTP.UsersManagement.API.Core.Models;
using Microsoft.Extensions.Logging;

namespace ITTP.UsersManagement.API.FunctionalTests;

public class UsersControllerTests : BaseFunctionalTest
{
    private readonly ILogger<UsersController> _logger;
        
    public UsersControllerTests(TestWebAppFactory factory) : base(factory)
    {
        _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<UsersController>();
    }

    [Fact]
    public void Login_Successful_ReturnsToken()
    {
        // Arrange
        LoginAndPasswordDto loginAndPasswordDto = new LoginAndPasswordDto("admin", "admin");

        // Act
        HttpResponseMessage response = _client.PostAsJsonAsync("/users/auth/login", loginAndPasswordDto).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        LoginTokenDto? result = response.Content.ReadFromJsonAsync<LoginTokenDto>().GetAwaiter().GetResult();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public void Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginAndPasswordDto("admin", "WrongPass");

        // Act
        var response = _client.PostAsJsonAsync("/users/auth/login", loginDto).GetAwaiter().GetResult();

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public void CreateUser_AsAdmin_Succeeds()
    {
        // Arrange
        string adminToken = GetAdminToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        CreateUserDto createDto = new CreateUserDto
        {
            Login = "newuser",
            Password = "password",
            Name = "NewUser",
            Gender = 1,
            Admin = false
        };

        // Act
        HttpResponseMessage response = _client.PostAsJsonAsync("/users", createDto).GetAwaiter().GetResult();
        _logger.LogInformation($"Status Code: {response.StatusCode}");
        string content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        _logger.LogInformation($"Response Content: {content}");
        response.EnsureSuccessStatusCode();
        LoginDto? result = response.Content.ReadFromJsonAsync<LoginDto>().GetAwaiter().GetResult();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("newuser", result.Login);
    }

    [Fact]
    public void CreateUser_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        string adminToken = GetAdminToken();
        LoginAndPasswordDto regular = CreateUser(adminToken, "regularuser", "regularpass");
        string regularToken = GetToken(regular.Login, regular.Password);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regularToken);
        var createDto = new CreateUserDto
        {
            Login = "anotheruser",
            Password = "password",
            Name = "AnotherUser",
            Gender = 1,
            Admin = false
        };

        // Act
        var response = _client.PostAsJsonAsync("/users", createDto).GetAwaiter().GetResult();

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public void CreateUser_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            Login = "unauthuser",
            Password = "password",
            Name = "UnauthUser",
            Gender = 1,
            Admin = false
        };

        // Act
        var response = _client.PostAsJsonAsync("/users", createDto).GetAwaiter().GetResult();

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public void UpdateUser_OwnInfo_AsRegularUser_Succeeds()
    {
        // Arrange
        string adminToken = GetAdminToken();
        LoginAndPasswordDto regular = CreateUser(adminToken, "updateuser", "updatepass");
        string userToken = GetToken(regular.Login, regular.Password);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);
        UserPersonalInfoDto updateDto = new UserPersonalInfoDto("UpdatedName", 2, null);

        // Act
        HttpResponseMessage response = _client.PutAsJsonAsync($"/users/{regular.Login}/updateUser", updateDto).GetAwaiter().GetResult();

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public void UpdateUser_OtherUser_AsRegularUser_ReturnsBadRequest()
    {
        // Arrange
        string adminToken = GetAdminToken();
        LoginAndPasswordDto user1 = CreateUser(adminToken, "user1", "pass1");
        LoginAndPasswordDto user2 = CreateUser(adminToken, "user2", "pass2");
        string user1Token = GetToken(user1.Login, user1.Password);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user1Token);
        UserPersonalInfoDto updateDto = new UserPersonalInfoDto("Hacked name", 2, null);

        // Act
        HttpResponseMessage response = _client.PutAsJsonAsync($"/users/{user2.Login}/updateUser", updateDto).GetAwaiter().GetResult();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public void UpdateUser_AsAdmin_Succeeds()
    {
        // Arrange
        string adminToken = GetAdminToken();
        LoginAndPasswordDto user = CreateUser(adminToken, "userupdate", "updatepass");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        UserPersonalInfoDto updateDto = new UserPersonalInfoDto("Adminupdated", 2, null);

        // Act
        HttpResponseMessage response = _client.PutAsJsonAsync($"/users/{user.Login}/updateUser", updateDto).GetAwaiter().GetResult();

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public void GetActiveUsers_AsAdmin_ReturnsUsers()
    {
        // Arrange
        string adminToken = GetAdminToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        HttpResponseMessage response = _client.GetAsync("/users/getActiveUsers").GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        User[] users = response.Content.ReadFromJsonAsync<User[]>().GetAwaiter().GetResult();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(users);
        Assert.Contains(users, u => u.Login == "admin");
    }

    [Fact]
    public void GetActiveUsers_AsRegularUser_ReturnsMethodNotAllowed()
    {
        // Arrange
        string adminToken = GetAdminToken();
        LoginAndPasswordDto regular = CreateUser(adminToken, "regularuser", "regularpass");
        string regularToken = GetToken(regular.Login, regular.Password);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regularToken);
        // Act
        HttpResponseMessage response = _client.GetAsync("/users").GetAwaiter().GetResult();

        // Assert
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public void GetUserByLogin_AsAdmin_Succeeds()
    {
        // Arrange
        string adminToken = GetAdminToken();
        LoginAndPasswordDto user = CreateUser(adminToken, "testuser", "testpass");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        HttpResponseMessage response = _client.GetAsync($"/users/getUserByLogin?login={user.Login}").GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        UserPersonalInfoDto result = response.Content.ReadFromJsonAsync<UserPersonalInfoDto>().GetAwaiter().GetResult();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
    }

    [Fact]
    public void GetUserByLoginAndPassword_AsUser_Succeeds()
    {
        // Arrange
        string adminToken = GetAdminToken();
        LoginAndPasswordDto user = CreateUser(adminToken, "verifyuser", "verifypass");
        string userToken = GetToken(user.Login, user.Password);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

        // Act
        HttpResponseMessage response = _client.GetAsync($"/users/{user.Login}?password={user.Password}").GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public void DeleteUser_SoftDelete_AsAdmin_Succeeds()
    {
        // Arrange
        string adminToken = GetAdminToken();
        LoginAndPasswordDto user = CreateUser(adminToken, "deleteuser", "deletepass");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        HttpResponseMessage response = _client.DeleteAsync($"/users/{user.Login}?softDelete=true").GetAwaiter().GetResult();

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public void RestoreUser_AsAdmin_Succeeds()
    {
        // Arrange
        string adminToken = GetAdminToken();
        LoginAndPasswordDto user = CreateUser(adminToken, "restoreuser", "restorepass");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        _client.DeleteAsync($"/users/{user.Login}?softDelete=true").GetAwaiter().GetResult();

        // Act
        HttpResponseMessage response = _client.PutAsync($"/users/{user.Login}/restore", null).GetAwaiter().GetResult();

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private string GetAdminToken()
    {
        return GetToken("admin", "admin");
    }

    private string GetToken(string login, string password)
    {
        LoginAndPasswordDto loginAndPasswordDto = new LoginAndPasswordDto(login, password);
        HttpResponseMessage response = _client.PostAsJsonAsync("/users/auth/login", loginAndPasswordDto).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        LoginTokenDto result = response.Content.ReadFromJsonAsync<LoginTokenDto>().GetAwaiter().GetResult();
        return result.Token;
    }

    private LoginAndPasswordDto CreateUser(string adminToken, string login, string password)
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        CreateUserDto createDto = new CreateUserDto
        {
            Login = login,
            Password = password,
            Name = $"{login}Name",
            Gender = 1,
            Admin = false
        };
        HttpResponseMessage response = _client.PostAsJsonAsync("/users", createDto).GetAwaiter().GetResult();
        return new LoginAndPasswordDto(login, password);
    }
}