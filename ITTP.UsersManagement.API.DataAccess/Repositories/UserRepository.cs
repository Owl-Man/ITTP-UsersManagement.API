using ITTP.UsersManagement.API.Core.Models;
using ITTP.UsersManagement.API.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ITTP.UsersManagement.API.DataAccess.Repositories;

public class UserRepository
{
    private readonly UsersManagementDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(UsersManagementDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public (Guid id, string error) Create(string login, string password, string name, int gender, DateTime? birthday, 
        bool admin, string createdBy)
    {
        try
        {
            User.ValidateData(out string validateError, login, password, name, gender);
        
            if (!string.IsNullOrEmpty(validateError)) return (Guid.Empty, validateError);
        
            UserEntity userEntity = new UserEntity();
        
            User user = User.CreateNew(userEntity.Id, login, password, name, gender, birthday, admin, createdBy);

            userEntity = user.ToEntity();
        
            _context.Users.Add(userEntity);
            _context.SaveChanges();
        
            return (user.Id, string.Empty);
        }
        catch (Exception ex)
        {
            string error = $"Error with creating user {login}";
            
            _logger.LogError(ex, error);
            return (Guid.Empty, error);
        }
    }

    public (Guid id, string error) UpdatePersonalInfo(Guid id, string name, int gender, DateTime? birthday, string modifiedBy)
    {
        try
        {
            User.ValidateData(out var validateError, name: name, gender:gender);
            
            if (!string.IsNullOrEmpty(validateError)) return (Guid.Empty, validateError);

            _context.Users
                .Where(u => u.Id == id)
                .ExecuteUpdate(s => s
                    .SetProperty(u => u.Name, u => name)
                    .SetProperty(u => u.Gender, u => gender)
                    .SetProperty(u => u.Birthday, u => birthday)
                    .SetProperty(u => u.ModifiedBy, u => modifiedBy)
                    .SetProperty(u => u.ModifiedOn, u => DateTime.UtcNow));
        
            return (id, string.Empty);
        }
        catch (Exception ex)
        {
            string error = $"No user found";
            
            _logger.LogError(ex, error);
            return (Guid.Empty, error);
        }
    }
    
    public (Guid id, string error) UpdateLogin(Guid id, string login, string modifiedBy)
    {
        try
        {
            User.ValidateData(out var validateError, login: login);
            
            if (!string.IsNullOrEmpty(validateError)) return (Guid.Empty, validateError);

            _context.Users
                .Where(u => u.Id == id)
                .ExecuteUpdate(s => s
                    .SetProperty(u => u.Login, u => login)
                    .SetProperty(u => u.ModifiedBy, u => modifiedBy)
                    .SetProperty(u => u.ModifiedOn, u => DateTime.UtcNow));
        
            return (id, string.Empty);
        }
        catch (Exception ex)
        {
            string error = $"No user found";
            
            _logger.LogError(ex, error);
            return (Guid.Empty, error);
        }
    }
    
    public (Guid id, string error) UpdatePassword(Guid id, string password, string modifiedBy)
    {
        try
        {
            User.ValidateData(out var validateError, password: password);
            
            if (!string.IsNullOrEmpty(validateError)) return (Guid.Empty, validateError);

            _context.Users
                .Where(u => u.Id == id)
                .ExecuteUpdate(s => s
                    .SetProperty(u => u.Password, u => password)
                    .SetProperty(u => u.ModifiedBy, u => modifiedBy)
                    .SetProperty(u => u.ModifiedOn, u => DateTime.UtcNow));
        
            return (id, string.Empty);
        }
        catch (Exception ex)
        {
            string error = $"No user found";
            
            _logger.LogError(ex, error);
            return (Guid.Empty, error);
        }
    }

    public (User? user, string error) GetByLogin(string login)
    {
        try
        {
            UserEntity userEntity = _context.Users.FirstOrDefault(u => u.Login == login);
        
            if (userEntity == null)
                return (null, "No user found");
        
            return (userEntity.ToUser(), string.Empty);
        }
        catch (Exception ex)
        {
            string error = $"No user found";
            
            _logger.LogError(ex, error);
            return (null, error);
        }
    }

    public (User? user, string error) GetByLoginAndPassword(string login, string password)
    {
        try
        {
            UserEntity userEntity = _context.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
        
            if (userEntity == null)
                return (null, "No user found");
        
            return (userEntity.ToUser(), string.Empty);
        }
        catch (Exception ex)
        {
            string error = $"No user found";
            
            _logger.LogError(ex, error);
            return (null, error);
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

    public List<User> GetUsersOlderThan(DateTime date)
    {
        List<UserEntity> userEntity = _context.Users
            .Where(u => u.RevokedOn == null && u.Birthday != null && u.Birthday >= date)
            .ToList();
        
        List<User> users = userEntity
            .Select(u => u.ToUser())
            .ToList();

        return users;
    }

    public (Guid id, string error) DeleteUserForce(Guid id)
    {
        try
        {
            _context.Users
                .Where(u => u.Id == id)
                .ExecuteDelete();
        
            return (id, string.Empty);
        }
        catch (Exception ex)
        {
            string error = $"No user found";
            
            _logger.LogError(ex, error);
            return (Guid.Empty, error);
        }
    }
    
    public (Guid id, string error) DeleteUser(Guid id, string revokedBy)
    {
        try
        {
            _context.Users
                .Where(u => u.Id == id)
                .ExecuteUpdate(s => s
                    .SetProperty(u => u.RevokedBy, u => revokedBy)
                    .SetProperty(u => u.RevokedOn, u => DateTime.UtcNow));
        
            return (id, string.Empty);
        }
        catch (Exception ex)
        {
            string error = $"No user found";
            
            _logger.LogError(ex, error);
            return (Guid.Empty, error);
        }
    }

    public (Guid id, string error) RecoverUser(Guid id)
    {
        try
        {
            _context.Users
                .Where(u => u.Id == id)
                .ExecuteUpdate(s => s
                    .SetProperty(u => u.RevokedBy, u => string.Empty)
                    .SetProperty(u => u.RevokedOn, u => null));
        
            return (id, string.Empty);
        }
        catch (Exception ex)
        {
            string error = $"No user found";
            
            _logger.LogError(ex, error);
            return (Guid.Empty, error);
        }
    }
}