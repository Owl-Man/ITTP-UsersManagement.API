using ITTP.UsersManagement.API.Core;
using ITTP.UsersManagement.API.Core.DTOs;
using ITTP.UsersManagement.API.Core.Interfaces;
using ITTP.UsersManagement.API.Core.Models;
using Microsoft.Extensions.Logging;

namespace ITTP.UsersManagement.API.Application.Services;

public class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;

    public UserService(ILogger<UserService> logger, IUserRepository userRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    public RetrievedId CreateUser(string login, string password, string name, int gender,
        DateTime? birthday,
        bool admin, string createdBy)
    {
        return _userRepository.Create(login, password, name, gender, birthday, admin, createdBy);
    }

    public RetrievedId UpdatePersonalInfo(string login, string name, int gender, DateTime? birthday,
        string modifiedBy)
    {
        RetrievedUser retrievedUser = _userRepository.GetByLogin(login);

        if (!string.IsNullOrEmpty(retrievedUser.error))
            return new RetrievedId(Guid.Empty, retrievedUser.error);
        
        if (retrievedUser.user.Login != modifiedBy && modifiedBy != "admin")
            return new RetrievedId(Guid.Empty, ErrorForm.AccessError(login));
            
        return _userRepository.UpdatePersonalInfo(retrievedUser.user.Id, name, gender, birthday, modifiedBy);
    }

    public RetrievedId UpdateLogin(string login, string newLogin, string modifiedBy)
    {
        RetrievedUser retrievedUser = _userRepository.GetByLogin(login);

        if (!string.IsNullOrEmpty(retrievedUser.error))
            return new RetrievedId(Guid.Empty, retrievedUser.error);
        
        return _userRepository.UpdateLogin(retrievedUser.user.Id, login, modifiedBy);
    }

    public RetrievedId UpdatePassword(string login, string password, string modifiedBy)
    {
        RetrievedUser retrievedUser = _userRepository.GetByLogin(login);

        if (!string.IsNullOrEmpty(retrievedUser.error))
            return new RetrievedId(Guid.Empty, retrievedUser.error);
        
        return _userRepository.UpdatePassword(retrievedUser.user.Id, password, modifiedBy);
    }

    public RetrievedUser GetByLoginAndPassword(string login, string password, string requestedBy)
    {
        RetrievedUser retrievedUser = _userRepository.GetByLoginAndPassword(login, password);
        
        if (retrievedUser.user != null && retrievedUser.user.Login != requestedBy)
            return new RetrievedUser(null, ErrorForm.AccessError(login));

        return retrievedUser;
    }
    
    public RetrievedUser GetUserByLogin(string login)
    {
        return _userRepository.GetByLogin(login);
    }

    public List<User> GetActiveUsers()
    {
        return _userRepository.GetActiveUsers();
    }

    public List<User> GetUsersOlderThan(int age)
    {
        return _userRepository.GetUsersOlderThan(age);
    }

    public RetrievedId DeleteUserForce(string login)
    {
        RetrievedUser retrievedUser = _userRepository.GetByLogin(login);

        if (!string.IsNullOrEmpty(retrievedUser.error))
            return new RetrievedId(Guid.Empty, retrievedUser.error);
        
        return _userRepository.DeleteUserForce(retrievedUser.user.Id);
    }

    public RetrievedId DeleteUser(string login, string revokedBy)
    {
        RetrievedUser retrievedUser = _userRepository.GetByLogin(login);

        if (!string.IsNullOrEmpty(retrievedUser.error))
            return new RetrievedId(Guid.Empty, retrievedUser.error);
        
        return _userRepository.DeleteUser(retrievedUser.user.Id, revokedBy);
    }

    public RetrievedId RestoreUser(string login)
    {
        RetrievedUser retrievedUser = _userRepository.GetByLogin(login);

        if (!string.IsNullOrEmpty(retrievedUser.error))
            return new RetrievedId(Guid.Empty, retrievedUser.error);
        
        return _userRepository.RestoreUser(retrievedUser.user.Id);
    }
}