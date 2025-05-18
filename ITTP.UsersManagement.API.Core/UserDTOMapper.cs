using ITTP.UsersManagement.API.Core.DTOs;
using ITTP.UsersManagement.API.Core.Models;

namespace ITTP.UsersManagement.API.Core;

public static class UserDTOMapper
{
    public static UserPersonalInfoDto ToUserPersonalInfo(this User user)
    {
        return new UserPersonalInfoDto(user.Name, user.Gender, user.Birthday);
    }
}