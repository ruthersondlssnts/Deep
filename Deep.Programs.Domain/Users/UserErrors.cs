using Deep.Common.Domain;

namespace Deep.Programs.Domain.Users;

public static class UserErrors
{
    public static Error NotFound(Guid userId) =>
        Error.NotFound("User.NotFound", $"The user with the identifier {userId} was not found");

    public static Error UserRoleNotFound(Guid userId, string role) =>
        Error.NotFound(
            "User.UserRoleNotFound",
            $"The user with the identifier {userId} and role '{role}' was not found");

    public static readonly Error UserAlreadyExists = Error.Problem(
        "User.AlreadyExists",
        "The user already exists");

    public static readonly Error UserRoleNotAllowed = Error.Problem(
          "UserRole.NotAllowed",
          "The role is not allowed");

    public static readonly Error InvalidRole = Error.Problem(
       "Role.Invalid",
       "The role is invalid");
}
