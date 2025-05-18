using ITTP.UsersManagement.API.Core.Models;

namespace ITTP.UsersManagement.API.Core.DTOs;

public record RetrievedUserDTO(User? user, string error);