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

    public RetrievedIdDto CreateUser(string login, string password, string name, int gender,
        DateTime? birthday,
        bool admin, string createdBy)
    {
        return _userRepository.Create(login, password, name, gender, birthday, admin, createdBy);
    }

    public RetrievedIdDto UpdatePersonalInfo(string login, string name, int gender, DateTime? birthday,
        string modifiedBy)
    {
        RetrievedUserDto retrievedUserDto = TryGetIfRegularUserCanAccessToRequestedUser(login, modifiedBy);

        if (!string.IsNullOrEmpty(retrievedUserDto.error))
            return new RetrievedIdDto(Guid.Empty, retrievedUserDto.error);
            
        return _userRepository.UpdatePersonalInfo(retrievedUserDto.user.Id, name, gender, birthday, modifiedBy);
    }

    public RetrievedIdDto UpdateLogin(string login, string newLogin, string modifiedBy)
    {
        RetrievedUserDto retrievedUserDto = TryGetIfRegularUserCanAccessToRequestedUser(login, modifiedBy);

        if (!string.IsNullOrEmpty(retrievedUserDto.error))
            return new RetrievedIdDto(Guid.Empty, retrievedUserDto.error);
        
        return _userRepository.UpdateLogin(retrievedUserDto.user.Id, newLogin, modifiedBy);
    }

    public RetrievedIdDto UpdatePassword(string login, string password, string modifiedBy)
    {
        RetrievedUserDto retrievedUserDto = TryGetIfRegularUserCanAccessToRequestedUser(login, modifiedBy);

        if (!string.IsNullOrEmpty(retrievedUserDto.error))
            return new RetrievedIdDto(Guid.Empty, retrievedUserDto.error);
        
        return _userRepository.UpdatePassword(retrievedUserDto.user.Id, password, modifiedBy);
    }
    
    public RetrievedUserDto GetUserByLogin(string login)
    {
        return _userRepository.GetByLogin(login);
    }

    public RetrievedUserDto GetByLoginAndPassword(string login, string password, string requestedBy)
    {
        return TryGetIfRegularUserCanAccessToRequestedUser(login,  requestedBy, false, password);
    }

    public List<User> GetActiveUsers()
    {
        return _userRepository.GetActiveUsers();
    }

    public List<User> GetUsersOlderThan(int age)
    {
        return _userRepository.GetUsersOlderThan(age);
    }

    public RetrievedIdDto DeleteUserForce(string login)
    {
        RetrievedUserDto retrievedUserDto = _userRepository.GetByLogin(login);

        if (!string.IsNullOrEmpty(retrievedUserDto.error))
            return new RetrievedIdDto(Guid.Empty, retrievedUserDto.error);
        
        return _userRepository.DeleteUserForce(retrievedUserDto.user.Id);
    }

    public RetrievedIdDto DeleteUser(string login, string revokedBy)
    {
        RetrievedUserDto retrievedUserDto = _userRepository.GetByLogin(login);

        if (!string.IsNullOrEmpty(retrievedUserDto.error))
            return new RetrievedIdDto(Guid.Empty, retrievedUserDto.error);
        
        return _userRepository.DeleteUser(retrievedUserDto.user.Id, revokedBy);
    }

    public RetrievedIdDto RestoreUser(string login)
    {
        RetrievedUserDto retrievedUserDto = _userRepository.GetByLogin(login);

        if (!string.IsNullOrEmpty(retrievedUserDto.error))
            return new RetrievedIdDto(Guid.Empty, retrievedUserDto.error);
        
        return _userRepository.RestoreUser(retrievedUserDto.user.Id);
    }

    private RetrievedUserDto TryGetIfRegularUserCanAccessToRequestedUser(string login, string requestedByUserLogin, bool isAdminAlsoHaveAccess=true, string password="")
    {
        RetrievedUserDto retrievedUserDto;
        
        retrievedUserDto = string.IsNullOrEmpty(password) ? _userRepository.GetByLogin(login) : _userRepository.GetByLoginAndPassword(login, password);
        
        if (!string.IsNullOrEmpty(retrievedUserDto.error))
            return new RetrievedUserDto(null,  retrievedUserDto.error);
        
        if ((retrievedUserDto.user.Login == requestedByUserLogin && retrievedUserDto.user.RevokedOn == null)
            || (isAdminAlsoHaveAccess && CheckIfUserIsAdmin(requestedByUserLogin)))
            return retrievedUserDto;

        _logger.LogError(ErrorForm.AccessError(login));
        return new RetrievedUserDto(null, ErrorForm.AccessError(login));
    }

    private bool CheckIfUserIsAdmin(string login)
    {
        RetrievedUserDto retrievedUserDto = _userRepository.GetByLogin(login);

        if (!string.IsNullOrEmpty(retrievedUserDto.error))
            return false;

        return retrievedUserDto.user.Admin;
    }
}