using ITTP.UsersManagement.API.Core.DTOs;
using ITTP.UsersManagement.API.Core.Models;

namespace ITTP.UsersManagement.API.Core.Interfaces;

public interface IUserRepository
{
    RetrievedIdDto Create(string login, string password, string name, int gender, DateTime? birthday, 
        bool admin, string createdBy);

    RetrievedIdDto UpdatePersonalInfo(Guid id, string name, int gender, DateTime? birthday, string modifiedBy);
    RetrievedIdDto UpdateLogin(Guid id, string login, string modifiedBy);
    RetrievedIdDto UpdatePassword(Guid id, string password, string modifiedBy);
    RetrievedUserDto GetByLogin(string login);
    RetrievedUserDto GetByLoginAndPassword(string login, string password);
    List<User> GetActiveUsers();
    List<User> GetUsersOlderThan(int age);
    RetrievedIdDto DeleteUserForce(Guid id);
    RetrievedIdDto DeleteUser(Guid id, string revokedBy);
    RetrievedIdDto RestoreUser(Guid id);
}