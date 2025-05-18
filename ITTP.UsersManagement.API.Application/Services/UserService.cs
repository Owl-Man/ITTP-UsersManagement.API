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

    public RetrievedIdDTO CreateUser(string login, string password, string name, int gender,
        DateTime? birthday,
        bool admin, string createdBy)
    {
        return _userRepository.Create(login, password, name, gender, birthday, admin, createdBy);
    }

    public RetrievedIdDTO UpdatePersonalInfo(string login, string name, int gender, DateTime? birthday,
        string modifiedBy)
    {
        RetrievedUserDTO retrievedUserDto = TryGetUserWithAccess(login, modifiedBy, true);

        if (!string.IsNullOrEmpty(retrievedUserDto.error))
            return new RetrievedIdDTO(Guid.Empty, retrievedUserDto.error);
            
        return _userRepository.UpdatePersonalInfo(retrievedUserDto.user.Id, name, gender, birthday, modifiedBy);
    }

    public RetrievedIdDTO UpdateLogin(string login, string newLogin, string modifiedBy)
    {
        RetrievedUserDTO retrievedUserDto = TryGetUserWithAccess(login, modifiedBy, true);

        if (!string.IsNullOrEmpty(retrievedUserDto.error))
            return new RetrievedIdDTO(Guid.Empty, retrievedUserDto.error);
        
        return _userRepository.UpdateLogin(retrievedUserDto.user.Id, login, modifiedBy);
    }

    public RetrievedIdDTO UpdatePassword(string login, string password, string modifiedBy)
    {
        RetrievedUserDTO retrievedUserDto = TryGetUserWithAccess(login, modifiedBy, true);

        if (!string.IsNullOrEmpty(retrievedUserDto.error))
            return new RetrievedIdDTO(Guid.Empty, retrievedUserDto.error);
        
        return _userRepository.UpdatePassword(retrievedUserDto.user.Id, password, modifiedBy);
    }
    
    public RetrievedUserDTO GetUserByLogin(string login, string requestedBy)
    {
        return TryGetUserWithAccess(login, requestedBy, true);
    }

    public RetrievedUserDTO GetByLoginAndPassword(string login, string password, string requestedBy)
    {
        return TryGetUserWithAccess(login,  requestedBy, false, password);
    }

    public List<User> GetActiveUsers()
    {
        return _userRepository.GetActiveUsers();
    }

    public List<User> GetUsersOlderThan(int age)
    {
        return _userRepository.GetUsersOlderThan(age);
    }

    public RetrievedIdDTO DeleteUserForce(string login)
    {
        RetrievedUserDTO retrievedUserDto = _userRepository.GetByLogin(login);

        if (!string.IsNullOrEmpty(retrievedUserDto.error))
            return new RetrievedIdDTO(Guid.Empty, retrievedUserDto.error);
        
        return _userRepository.DeleteUserForce(retrievedUserDto.user.Id);
    }

    public RetrievedIdDTO DeleteUser(string login, string revokedBy)
    {
        RetrievedUserDTO retrievedUserDto = _userRepository.GetByLogin(login);

        if (!string.IsNullOrEmpty(retrievedUserDto.error))
            return new RetrievedIdDTO(Guid.Empty, retrievedUserDto.error);
        
        return _userRepository.DeleteUser(retrievedUserDto.user.Id, revokedBy);
    }

    public RetrievedIdDTO RestoreUser(string login)
    {
        RetrievedUserDTO retrievedUserDto = _userRepository.GetByLogin(login);

        if (!string.IsNullOrEmpty(retrievedUserDto.error))
            return new RetrievedIdDTO(Guid.Empty, retrievedUserDto.error);
        
        return _userRepository.RestoreUser(retrievedUserDto.user.Id);
    }

    private RetrievedUserDTO TryGetUserWithAccess(string login, string requestedByUserLogin, bool isAdminHaveAccess, string password="")
    {
        RetrievedUserDTO retrievedUserDto;
        
        retrievedUserDto = string.IsNullOrEmpty(password) ? _userRepository.GetByLogin(login) : _userRepository.GetByLoginAndPassword(login, password);
        
        if (!string.IsNullOrEmpty(retrievedUserDto.error))
            return new RetrievedUserDTO(null,  retrievedUserDto.error);
        
        if ((retrievedUserDto.user.Login == requestedByUserLogin && retrievedUserDto.user.RevokedOn == null)
            || (isAdminHaveAccess && requestedByUserLogin == "admin"))
            return retrievedUserDto;

        return new RetrievedUserDTO(null, ErrorForm.AccessError(login));
    }
}