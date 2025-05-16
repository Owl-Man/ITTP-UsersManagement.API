using System.Net;
using System.Net.Http.Json;
using ITTP.UsersManagement.API.Core.DTOs;
using ITTP.UsersManagement.API.Core.Models;
using ITTP.UsersManagement.API.FunctionalTests;
using Microsoft.Extensions.Logging;
using UserManagementApi.Controllers;

namespace UserManagementApi.FunctionalTests
{
    public class UsersControllerTests : BaseIntegrationTest
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
            var loginDto = new UserLoginDto { Login = "Admin", Password = "Admin123" };

            // Act
            HttpResponseMessage response = _client.PostAsJsonAsync("/users/login", loginDto).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            LoginResponse? result = response.Content.ReadFromJsonAsync<LoginResponse>().GetAwaiter().GetResult();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
        }

        [Fact]
        public void Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new UserLoginDto { Login = "Admin", Password = "WrongPass" };

            // Act
            var response = _client.PostAsJsonAsync("/users/login", loginDto).GetAwaiter().GetResult();

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public void CreateUser_AsAdmin_Succeeds()
        {
            // Arrange
            var adminToken = GetAdminToken();
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
            var response = _client.PostAsJsonAsync("/users", createDto).GetAwaiter().GetResult();
            Console.WriteLine($"Status Code: {response.StatusCode}");
            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Console.WriteLine($"Response Content: {content}");
            response.EnsureSuccessStatusCode();
            CreateUserResponse? result = response.Content.ReadFromJsonAsync<CreateUserResponse>().GetAwaiter().GetResult();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
            Assert.Equal("newuser", result.login);
        }

        [Fact]
        public void CreateUser_AsRegularUser_ReturnsForbidden()
        {
            // Arrange
            var adminToken = GetAdminToken();
            var regularUser = CreateRegularUser(adminToken, "regularuser", "regularpass");
            var regularToken = GetToken(regularUser.Login, regularUser.Password);
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
                Name = "Unauth User",
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
            var adminToken = GetAdminToken();
            var regularUser = CreateRegularUser(adminToken, "updateuser", "updatepass");
            var userToken = GetToken(regularUser.Login, regularUser.Password);
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);
            var updateDto = new UserPersonalInfoDto("Updated Name", 2, null);

            // Act
            var response = _client.PutAsJsonAsync($"/users/{regularUser.Login}", updateDto).GetAwaiter().GetResult();

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public void UpdateUser_OtherUser_AsRegularUser_ReturnsBadRequest()
        {
            // Arrange
            var adminToken = GetAdminToken();
            var user1 = CreateRegularUser(adminToken, "user1", "pass1");
            var user2 = CreateRegularUser(adminToken, "user2", "pass2");
            var user1Token = GetToken(user1.Login, user1.Password);
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user1Token);
            var updateDto = new UserPersonalInfoDto("Hacked name", 2, null);

            // Act
            var response = _client.PutAsJsonAsync($"/users/{user2.Login}", updateDto).GetAwaiter().GetResult();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public void UpdateUser_AsAdmin_Succeeds()
        {
            // Arrange
            var adminToken = GetAdminToken();
            var user = CreateRegularUser(adminToken, "usertoupdate", "updatepass");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            var updateDto = new UserPersonalInfoDto("Admin updated", 2, null);

            // Act
            var response = _client.PutAsJsonAsync($"/users/{user.Login}", updateDto).GetAwaiter().GetResult();

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public void GetActiveUsers_AsAdmin_ReturnsUsers()
        {
            // Arrange
            var adminToken = GetAdminToken();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            // Act
            var response = _client.GetAsync("/users").GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            var users = response.Content.ReadFromJsonAsync<User[]>().GetAwaiter().GetResult();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(users);
            Assert.Contains(users, u => u.Login == "Admin");
        }

        [Fact]
        public void GetActiveUsers_AsRegularUser_ReturnsForbidden()
        {
            // Arrange
            string adminToken = GetAdminToken();
            UserCredentials regularUser = CreateRegularUser(adminToken, "regularuser", "regularpass");
            string regularToken = GetToken(regularUser.Login, regularUser.Password);
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regularToken);
            // Act
            var response = _client.GetAsync("/users").GetAwaiter().GetResult();

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public void GetUserByLogin_AsAdmin_Succeeds()
        {
            // Arrange
            string adminToken = GetAdminToken();
            UserCredentials user = CreateRegularUser(adminToken, "testuser", "testpass");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

            // Act
            HttpResponseMessage response = _client.GetAsync($"/users/{user.Login}").GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            UserPersonalInfoDto? result = response.Content.ReadFromJsonAsync<UserPersonalInfoDto>().GetAwaiter().GetResult();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
        }

        [Fact]
        public void GetUserByLoginAndPassword_AsUser_Succeeds()
        {
            // Arrange
            string adminToken = GetAdminToken();
            UserCredentials user = CreateRegularUser(adminToken, "verifyuser", "verifypass");
            string userToken = GetToken(user.Login, user.Password);
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);
            UserLoginDto loginDto = new UserLoginDto { Login = user.Login, Password = user.Password };

            // Act
            HttpResponseMessage response = _client.PostAsJsonAsync($"/users/{user.Login}/getByLoginAndPassword", loginDto).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public void DeleteUser_SoftDelete_AsAdmin_Succeeds()
        {
            // Arrange
            string adminToken = GetAdminToken();
            UserCredentials user = CreateRegularUser(adminToken, "deleteuser", "deletepass");
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
            UserCredentials user = CreateRegularUser(adminToken, "restoreuser", "restorepass");
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
            UserLoginDto loginDto = new UserLoginDto { Login = login, Password = password };
            HttpResponseMessage response = _client.PostAsJsonAsync("/users/login", loginDto).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            LoginResponse? result = response.Content.ReadFromJsonAsync<LoginResponse>().GetAwaiter().GetResult();
            return result.Token;
        }

        private UserCredentials CreateRegularUser(string adminToken, string login, string password)
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            CreateUserDto createDto = new CreateUserDto
            {
                Login = login,
                Password = password,
                Name = $"{login} Name",
                Gender = 1,
                Admin = false
            };
            HttpResponseMessage response = _client.PostAsJsonAsync("/users", createDto).GetAwaiter().GetResult();
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
    }
}