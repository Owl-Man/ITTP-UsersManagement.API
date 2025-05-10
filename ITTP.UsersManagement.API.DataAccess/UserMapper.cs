using ITTP.UsersManagement.API.Core.Models;
using ITTP.UsersManagement.API.DataAccess.Entities;

namespace ITTP.UsersManagement.API.DataAccess;

public static class UserMapper
{
    public static UserEntity ToEntity(this User user)
    {
        return new UserEntity()
        {
            Id = user.Id,
            Login = user.Login,
            Password = user.Password,
            Name = user.Name,
            Gender = user.Gender,
            Birthday = user.Birthday,
            Admin = user.Admin,
            CreatedBy = user.CreatedBy,
            CreatedOn = user.CreatedOn,
            ModifiedBy = user.ModifiedBy,
            ModifiedOn = user.ModifiedOn,
            RevokedBy = user.RevokedBy,
            RevokedOn = user.RevokedOn
        };
    }

    public static User ToUser(this UserEntity userEntity)
    {
        return new User(userEntity.Id, userEntity.Login, userEntity.Password, userEntity.Name, userEntity.Gender,
            userEntity.Birthday, userEntity.Admin, userEntity.CreatedOn, userEntity.CreatedBy, userEntity.ModifiedOn,
            userEntity.ModifiedBy, userEntity.RevokedOn, userEntity.RevokedBy);
    }
}