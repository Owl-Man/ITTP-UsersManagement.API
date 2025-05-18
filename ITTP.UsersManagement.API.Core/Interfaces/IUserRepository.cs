using ITTP.UsersManagement.API.Core.DTOs;
using ITTP.UsersManagement.API.Core.Models;

namespace ITTP.UsersManagement.API.Core.Interfaces;

public interface IUserRepository
{
    RetrievedIdDTO Create(string login, string password, string name, int gender, DateTime? birthday, 
        bool admin, string createdBy);

    RetrievedIdDTO UpdatePersonalInfo(Guid id, string name, int gender, DateTime? birthday, string modifiedBy);
    RetrievedIdDTO UpdateLogin(Guid id, string login, string modifiedBy);
    RetrievedIdDTO UpdatePassword(Guid id, string password, string modifiedBy);
    RetrievedUserDTO GetByLogin(string login);
    RetrievedUserDTO GetByLoginAndPassword(string login, string password);
    List<User> GetActiveUsers();
    List<User> GetUsersOlderThan(int age);
    RetrievedIdDTO DeleteUserForce(Guid id);
    RetrievedIdDTO DeleteUser(Guid id, string revokedBy);
    RetrievedIdDTO RestoreUser(Guid id);
}