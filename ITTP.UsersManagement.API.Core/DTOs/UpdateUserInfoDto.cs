namespace ITTP.UsersManagement.API.Core.DTOs;

public class UpdateUserInfoDto
{
    public string Name { get; set; }
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
}