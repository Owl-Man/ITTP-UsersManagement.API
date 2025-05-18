using ITTP.UsersManagement.API.Core.Models;

namespace ITTP.UsersManagement.API.Core.DTOs;

public record RetrievedUserDto(User? user, string error);