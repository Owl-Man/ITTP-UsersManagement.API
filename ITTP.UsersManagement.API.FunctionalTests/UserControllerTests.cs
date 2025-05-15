using System.Net;
using System.Net.Http.Json;
using ITTP.UsersManagement.API.Core.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ITTP.UsersManagement.API.FunctionalTests;

public class UserControllerTests : BaseIntegrationTest
{
    public UserControllerTests(TestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Login_Successful_ReturnsToken()
    {
        // Arrange
        var loginDto = new UserLoginDto { Login = "Admin", Password = "Admin123" };

        // Act
        var response = await _client.PostAsJsonAsync("/users/login", loginDto);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new UserLoginDto { Login = "Admin", Password = "WrongPass" };

        // Act
        var response = await _client.PostAsJsonAsync("/users/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_AsAdmin_Succeeds()
    {
        // Arrange
        var adminToken = await GetAdminToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        var createDto = new CreateUserDto
        {
            Login = "newuser",
            Password = "password",
            Name = "New User",
            Gender = 1,
            Admin = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/users", createDto);
        var result = await response.Content.ReadFromJsonAsync<CreateUserResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("newuser", result.login);
    }

    [Fact]
    public async Task CreateUser_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        var adminToken = await GetAdminToken();
        var regularUser = await CreateRegularUser(adminToken, "regularuser", "regularpass");
        var regularToken = await GetToken(regularUser.Login, regularUser.Password);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regularToken);
        var createDto = new CreateUserDto
        {
            Login = "anotheruser",
            Password = "password",
            Name = "Another User",
            Gender = 1,
            Admin = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/users", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            Login = "unauthuser",
            Password = "password",
            Name = "Unauth User",
            Gender = 1,
            Admin = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/users", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_OwnInfo_AsRegularUser_Succeeds()
    {
        // Arrange
        var adminToken = await GetAdminToken();
        var regularUser = await CreateRegularUser(adminToken, "updateuser", "updatepass");
        var userToken = await GetToken(regularUser.Login, regularUser.Password);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);
        UserPersonalInfoDto updateDto = new UserPersonalInfoDto("Updated Name", 2, null);

        // Act
        var response = await _client.PutAsJsonAsync($"/users/{regularUser.Login}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_OtherUser_AsRegularUser_ReturnsBadRequest()
    {
        // Arrange
        var adminToken = await GetAdminToken();
        var user1 = await CreateRegularUser(adminToken, "user1", "pass1");
        var user2 = await CreateRegularUser(adminToken, "user2", "pass2");
        var user1Token = await GetToken(user1.Login, user1.Password);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user1Token);
        UserPersonalInfoDto updateDto = new UserPersonalInfoDto("Hacked Name", 2, null);

        // Act
        var response = await _client.PutAsJsonAsync($"/users/{user2.Login}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_AsAdmin_Succeeds()
    {
        // Arrange
        var adminToken = await GetAdminToken();
        var user = await CreateRegularUser(adminToken, "usertoupdate", "updatepass");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        UserPersonalInfoDto updateDto = new UserPersonalInfoDto("Admin Updated", 0, null);

        // Act
        var response = await _client.PutAsJsonAsync($"/users/{user.Login}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetActiveUsers_AsAdmin_ReturnsUsers()
    {
        // Arrange
        var adminToken = await GetAdminToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await _client.GetAsync("/users");
        var users = await response.Content.ReadFromJsonAsync<UserResponseDto[]>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(users);
        Assert.Contains(users, u => u.Login == "Admin");
    }

    [Fact]
    public async Task GetActiveUsers_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        var adminToken = await GetAdminToken();
        var regularUser = await CreateRegularUser(adminToken, "regularuser", "regularpass");
        var regularToken = await GetToken(regularUser.Login, regularUser.Password);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regularToken);

        // Act
        var response = await _client.GetAsync("/users");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUserByLogin_AsAdmin_Succeeds()
    {
        // Arrange
        var adminToken = await GetAdminToken();
        UserCredentials user = await CreateRegularUser(adminToken, "testuser", "testpass");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/users/{user.Login}");
        var result = await response.Content.ReadFromJsonAsync<UserPersonalInfoDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        //Assert.Equal(user.Name, result.Name);
    }

    [Fact]
    public async Task GetUserByLoginAndPassword_AsUser_Succeeds()
    {
        // Arrange
        var adminToken = await GetAdminToken();
        var user = await CreateRegularUser(adminToken, "verifyuser", "verifypass");
        var userToken = await GetToken(user.Login, user.Password);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);
        var loginDto = new UserLoginDto { Login = user.Login, Password = user.Password };

        // Act
        var response = await _client.PostAsJsonAsync($"/users/{user.Login}/getByLoginAndPassword", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_SoftDelete_AsAdmin_Succeeds()
    {
        // Arrange
        var adminToken = await GetAdminToken();
        var user = await CreateRegularUser(adminToken, "deleteuser", "deletepass");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await _client.DeleteAsync($"/users/{user.Login}?softDelete=true");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RestoreUser_AsAdmin_Succeeds()
    {
        // Arrange
        var adminToken = await GetAdminToken();
        var user = await CreateRegularUser(adminToken, "restoreuser", "restorepass");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        await _client.DeleteAsync($"/users/{user.Login}?softDelete=true");

        // Act
        var response = await _client.PutAsync($"/users/{user.Login}/restore", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<string> GetAdminToken()
    {
        return await GetToken("Admin", "Admin123");
    }

    private async Task<string> GetToken(string login, string password)
    {
        var loginDto = new UserLoginDto { Login = login, Password = password };
        var response = await _client.PostAsJsonAsync("/users/login", loginDto);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result.Token;
    }

    private async Task<UserCredentials> CreateRegularUser(string adminToken, string login, string password)
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        var createDto = new CreateUserDto
        {
            Login = login,
            Password = password,
            Name = $"{login} Name",
            Gender = 1,
            Admin = false
        };
        var response = await _client.PostAsJsonAsync("/users", createDto);
        response.EnsureSuccessStatusCode();
        return new UserCredentials(login, password);
    }

    private record UserCredentials(string Login, string Password);

    private class LoginResponse
    {
        public string Token { get; set; }
    }

    private class CreateUserResponse
    {
        public string login { get; set; }
    }

    private class UserResponseDto
    {
        public string Login { get; set; }
        public string Name { get; set; }
        public int Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public bool Admin { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
    }
}