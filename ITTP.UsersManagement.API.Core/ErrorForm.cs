namespace ITTP.UsersManagement.API.Core;

public static class ErrorForm
{
    public static string NoUsersFound(Guid id) => $"No users found with ID {id}";
    public static string NoUsersFound(string login) => $"No users found with login {login}";
    public static string UserCreateError(string login) => $"Error with creating user with login {login}";
}