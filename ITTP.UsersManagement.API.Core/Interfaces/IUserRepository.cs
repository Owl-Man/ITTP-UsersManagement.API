using ITTP.UsersManagement.API.Core.DTOs;
using ITTP.UsersManagement.API.Core.Models;

namespace ITTP.UsersManagement.API.Core.Interfaces;

public interface IUserRepository
{
    RetrievedId Create(string login, string password, string name, int gender, DateTime? birthday, 
        bool admin, string createdBy);

    RetrievedId UpdatePersonalInfo(Guid id, string name, int gender, DateTime? birthday, string modifiedBy);
    RetrievedId UpdateLogin(Guid id, string login, string modifiedBy);
    RetrievedId UpdatePassword(Guid id, string password, string modifiedBy);
    RetrievedUser GetByLogin(string login);
    RetrievedUser GetByLoginAndPassword(string login, string password);
    List<User> GetActiveUsers();
    List<User> GetUsersOlderThan(int age);
    RetrievedId DeleteUserForce(Guid id);
    RetrievedId DeleteUser(Guid id, string revokedBy);
    RetrievedId RestoreUser(Guid id);
}