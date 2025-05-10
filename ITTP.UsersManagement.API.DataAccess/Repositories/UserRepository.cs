using ITTP.UsersManagement.API.Core.Models;
using ITTP.UsersManagement.API.DataAccess.Entities;

namespace ITTP.UsersManagement.API.DataAccess.Repositories;

public class UserRepository
{
    private readonly UsersManagementDbContext _context;

    public UserRepository(UsersManagementDbContext context)
    {
        _context = context;
    }

    public (Guid id, string error) Create(string login, string password, string name, int gender, DateTime? birthday, bool admin, string createdBy)
    {
        User.ValidateData(login, password, name, gender, out var validateError);
        
        if (!string.IsNullOrEmpty(validateError)) return (Guid.Empty, validateError);
        
        UserEntity userEntity = new UserEntity();
        
        User user = User.CreateNew(userEntity.Id, login, password, name, gender, birthday, admin, createdBy);
        
        userEntity.Id = user.Id;
        userEntity.Login = user.Login;
        userEntity.Password = user.Password;
        userEntity.Gender = user.Gender;
        userEntity.Birthday = user.Birthday;
        userEntity.Admin = user.Admin;
        userEntity.CreatedBy = user.CreatedBy;
        userEntity.CreatedOn = user.CreatedOn;
        userEntity.ModifiedBy = user.ModifiedBy;
        userEntity.ModifiedOn = user.ModifiedOn;
        userEntity.RevokedOn = user.RevokedOn;
        userEntity.RevokedBy = user.RevokedBy;
        
        _context.Users.Add(userEntity);
        _context.SaveChanges();
        
        return (user.Id, string.Empty);
    }

    public User GetByLogin(string login)
    {
        UserEntity userEntity = _context.Users.FirstOrDefault(u => u.Login == login);
        
        User user = new User(userEntity.Id, userEntity.Login, userEntity.Password, userEntity.Name, userEntity.Gender,
            userEntity.Birthday, userEntity.Admin, userEntity.CreatedOn, userEntity.CreatedBy, userEntity.ModifiedOn, 
            userEntity.ModifiedBy, userEntity.RevokedOn, userEntity.RevokedBy);
        
        return user;
    }

    public User GetByLoginAndPassword(string login, string password)
    {
        UserEntity userEntity = _context.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
        
        User user = new User(userEntity.Id, userEntity.Login, userEntity.Password, userEntity.Name, userEntity.Gender,
            userEntity.Birthday, userEntity.Admin, userEntity.CreatedOn, userEntity.CreatedBy, userEntity.ModifiedOn, 
            userEntity.ModifiedBy, userEntity.RevokedOn, userEntity.RevokedBy);
        
        return user;
    }

    /*public List<User> GetActivateUsers()
    {
        List<UserEntity> userEntity = _context.Users
            .Where(u => u.RevokedOn == null)
            .OrderBy(u => u.CreatedOn)`
            .ToList();
        
        //List<User> users = userEntity
        //    .Select(u => new User(u.Id, u.Login))
        
        
    }*/
}