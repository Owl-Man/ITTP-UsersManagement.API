using ITTP.UsersManagement.API.Core.Models;

namespace ITTP.UsersManagement.API.Core.DTOs;

public record RetrievedUser(User? user, string error);