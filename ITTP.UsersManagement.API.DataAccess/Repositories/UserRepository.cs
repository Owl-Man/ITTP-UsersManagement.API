using ITTP.UsersManagement.API.Core;
using ITTP.UsersManagement.API.Core.DTOs;
using ITTP.UsersManagement.API.Core.Interfaces;
using ITTP.UsersManagement.API.Core.Models;
using ITTP.UsersManagement.API.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ITTP.UsersManagement.API.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UsersManagementDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(UsersManagementDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public RetrievedIdDto Create(string login, string password, string name, int gender, DateTime? birthday, 
        bool admin, string createdBy)
    {
        try
        {
            User.ValidateData(out string validateError, login, password, name, gender);
        
            if (!string.IsNullOrEmpty(validateError)) return new RetrievedIdDto(Guid.Empty, validateError);

            if (_context.Users.Any(u => u.Login == login))
                return new RetrievedIdDto(Guid.Empty, ErrorForm.UserExists(login));
        
            UserEntity userEntity = new UserEntity();

            userEntity = User.CreateNew(userEntity.Id, login, password, name, gender, birthday, admin, createdBy).ToEntity();
        
            _context.Users.Add(userEntity);
            _context.SaveChanges();
        
            return new RetrievedIdDto(userEntity.Id, string.Empty);
        }
        catch (Exception ex)
        {
            string error = ErrorForm.UserCreateError(login);
            
            _logger.LogError(ex, error);
            return new RetrievedIdDto(Guid.Empty, error);
        }
    }

    public RetrievedIdDto UpdatePersonalInfo(Guid id, string name, int gender, DateTime? birthday, string modifiedBy)
    {
        try
        {
            User.ValidateData(out var validateError, name: name, gender:gender);
            
            if (!string.IsNullOrEmpty(validateError)) return new RetrievedIdDto(Guid.Empty, validateError);

            _context.Users
                .Where(u => u.Id == id)
                .ExecuteUpdate(s => s
                    .SetProperty(u => u.Name, u => name)
                    .SetProperty(u => u.Gender, u => gender)
                    .SetProperty(u => u.Birthday, u => birthday)
                    .SetProperty(u => u.ModifiedBy, u => modifiedBy)
                    .SetProperty(u => u.ModifiedOn, u => DateTime.UtcNow));
        
            return new RetrievedIdDto(id, string.Empty);
        }
        catch (Exception ex)
        {
            string error = ErrorForm.NoUsersFound(id);
            
            _logger.LogError(ex, error);
            return new RetrievedIdDto(Guid.Empty, error);
        }
    }
    
    public RetrievedIdDto UpdateLogin(Guid id, string login, string modifiedBy)
    {
        try
        {
            User.ValidateData(out var validateError, login: login);
            
            if (!string.IsNullOrEmpty(validateError)) return new RetrievedIdDto(Guid.Empty, validateError);
            
            if (_context.Users.Any(u => u.Login == login))
                return new RetrievedIdDto(Guid.Empty, ErrorForm.UserExists(login));

            _context.Users
                .Where(u => u.Id == id)
                .ExecuteUpdate(s => s
                    .SetProperty(u => u.Login, u => login)
                    .SetProperty(u => u.ModifiedBy, u => modifiedBy)
                    .SetProperty(u => u.ModifiedOn, u => DateTime.UtcNow));
        
            return new RetrievedIdDto(id, string.Empty);
        }
        catch (Exception ex)
        {
            string error = ErrorForm.NoUsersFound(id);
            
            _logger.LogError(ex, error);
            return new RetrievedIdDto(Guid.Empty, error);
        }
    }
    
    public RetrievedIdDto UpdatePassword(Guid id, string password, string modifiedBy)
    {
        try
        {
            User.ValidateData(out var validateError, password: password);
            
            if (!string.IsNullOrEmpty(validateError)) return new RetrievedIdDto(Guid.Empty, validateError);

            _context.Users
                .Where(u => u.Id == id)
                .ExecuteUpdate(s => s
                    .SetProperty(u => u.Password, u => password)
                    .SetProperty(u => u.ModifiedBy, u => modifiedBy)
                    .SetProperty(u => u.ModifiedOn, u => DateTime.UtcNow));
        
            return new RetrievedIdDto(id, string.Empty);
        }
        catch (Exception ex)
        {
            string error = ErrorForm.NoUsersFound(id);
            
            _logger.LogError(ex, error);
            return new RetrievedIdDto(Guid.Empty, error);
        }
    }

    public RetrievedUserDto GetByLogin(string login)
    {
        try
        {
            UserEntity userEntity = _context.Users.FirstOrDefault(u => u.Login == login);
        
            if (userEntity == null)
                return new RetrievedUserDto(null, ErrorForm.NoUsersFound(login));
        
            return new RetrievedUserDto(userEntity.ToUser(), string.Empty);
        }
        catch (Exception ex)
        {
            string error = ErrorForm.NoUsersFound(login);
            
            _logger.LogError(ex, error);
            return new RetrievedUserDto(null, error);
        }
    }

    public RetrievedUserDto GetByLoginAndPassword(string login, string password)
    {
        try
        {
            UserEntity userEntity = _context.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
        
            if (userEntity == null)
                return new RetrievedUserDto(null, ErrorForm.NoUsersFound(login));
        
            return new RetrievedUserDto(userEntity.ToUser(), string.Empty);
        }
        catch (Exception ex)
        {
            string error = ErrorForm.NoUsersFound(login);
            
            _logger.LogError(ex, error);
            return new RetrievedUserDto(null, error);
        }
    }

    public List<User> GetActiveUsers()
    {
        List<UserEntity> userEntity = _context.Users
            .Where(u => u.RevokedOn == null)
            .OrderBy(u => u.CreatedOn)
            .ToList();
        
        List<User> users = userEntity
            .Select(u => u.ToUser())
            .ToList();
        
        return users;
    }

    public List<User> GetUsersOlderThan(int age)
    {
        DateTime today = DateTime.Today;
        
        List<UserEntity> userEntity = _context.Users
            .Where(u => u.RevokedOn == null && u.Birthday != null &&
                        today.Year - u.Birthday.Value.Year - (u.Birthday.Value.Date > today.AddYears(-(today.Year - u.Birthday.Value.Year)) ? 1 : 0) >= age)
            .ToList();
        
        List<User> users = userEntity
            .Select(u => u.ToUser())
            .ToList();

        return users;
    }
    
    
    public RetrievedIdDto DeleteUserForce(Guid id)
    {
        try
        {
            _context.Users
                .Where(u => u.Id == id)
                .ExecuteDelete();
        
            return new RetrievedIdDto(id, string.Empty);
        }
        catch (Exception ex)
        {
            string error = ErrorForm.NoUsersFound(id);
            
            _logger.LogError(ex, error);
            return new RetrievedIdDto(Guid.Empty, error);
        }
    }
    
    public RetrievedIdDto DeleteUser(Guid id, string revokedBy)
    {
        try
        {
            _context.Users
                .Where(u => u.Id == id)
                .ExecuteUpdate(s => s
                    .SetProperty(u => u.RevokedBy, u => revokedBy)
                    .SetProperty(u => u.RevokedOn, u => DateTime.UtcNow));
        
            return new RetrievedIdDto(id, string.Empty);
        }
        catch (Exception ex)
        {
            string error = ErrorForm.NoUsersFound(id);
            
            _logger.LogError(ex, error);
            return new RetrievedIdDto(Guid.Empty, error);
        }
    }

    public RetrievedIdDto RestoreUser(Guid id)
    {
        try
        {
            _context.Users
                .Where(u => u.Id == id)
                .ExecuteUpdate(s => s
                    .SetProperty(u => u.RevokedBy, u => string.Empty)
                    .SetProperty(u => u.RevokedOn, u => null));
        
            return new RetrievedIdDto(id, string.Empty);
        }
        catch (Exception ex)
        {
            string error = ErrorForm.NoUsersFound(id);
            
            _logger.LogError(ex, error);
            return new RetrievedIdDto(Guid.Empty, error);
        }
    }
}