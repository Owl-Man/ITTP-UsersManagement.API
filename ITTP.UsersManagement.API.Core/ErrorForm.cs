namespace ITTP.UsersManagement.API.Core;

public static class ErrorForm
{
    public static string NoUsersFound(Guid id) => $"No users found with ID {id}";
    public static string NoUsersFound(string login) => $"No users found with login {login}";
    public static string FormatError() => $"Can only contain letters and numbers";
    public static string UserCreateError(string login) => $"Error with creating user with login {login}";
    public static string AccessError(string login) => $"The user with the login {login} does not have access to this";
    public static string AuthenticateError() => $"Authentication error";
    public static string UserExists(string login) => $"User with login {login} already exists";
}