using System.Text.RegularExpressions;

namespace ITTP.UsersManagement.API.Core.Models;

public class User
{
    public Guid Id { get; }
    public string Login { get; }
    public string Password { get; }
        
    public string Name { get; }
    public int Gender { get; }
    public DateTime? Birthday { get; }
        
    public bool Admin { get; }
    public DateTime CreatedOn { get; }
    public string CreatedBy { get; }
    public DateTime ModifiedOn { get; }
    public string ModifiedBy { get; }
    public DateTime? RevokedOn { get; }
    public string RevokedBy { get; }
    
    public User(Guid id, string login, string password, string name, int gender, DateTime? birthday, bool admin,
        DateTime createdOn, string createdBy, DateTime modifiedOn, string modifiedBy, DateTime? revokedOn, string revokedBy)
    {
        Id = id;
        Login = login;
        Password = password;
        Name = name;
        Gender = gender;
        Birthday = birthday;
        Admin = admin;
        CreatedOn = createdOn;
        CreatedBy = createdBy;
        ModifiedOn = modifiedOn;
        ModifiedBy = modifiedBy;
        RevokedOn = revokedOn;
        RevokedBy = revokedBy;
    }

    public static User CreateNew(Guid id, string login, string password, string name, int gender,
        DateTime? birthday, bool admin, string createdBy)
    {
        User user = new User(id, login, password, name, gender,
            birthday, admin, DateTime.UtcNow, createdBy, DateTime.UtcNow,
            createdBy, null, string.Empty);

        return user;
    }

    public static bool ValidateData(out string validateError, string login = "def",
        string name = "def", int gender = 2)
    {
        validateError = string.Empty;
        
        if (!Regex.IsMatch(login, @"^[a-zA-Z0-9]+$") ||
            !Regex.IsMatch(name, @"^[a-zA-Z0-9]+$"))
        {
            validateError = "can only contain letters and numbers";
            return false;
        }
        
        if (gender != 0 && gender != 1 && gender != 2)
        {
            validateError = "gender must be either 0 or 1 or 2";
            return false;
        }
        
        return true;
    }

    public static bool ValidatePassword(string password)
    {
        if (!Regex.IsMatch(password, @"^[a-zA-Z0-9]+$"))
            return false;
        
        return true;
    }
}