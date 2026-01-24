using Deep.Common.Domain;

namespace Deep.Programs.Domain.Users;

public static class UserErrors
{
    public static Error NotFound(Guid userId) =>
        Error.NotFound("User.NotFound", $"The user with the identifier {userId} was not found");

    public static readonly Error UserAlreadyExists = Error.Problem(
        "User.AlreadyExists",
        "The user already exists");
}
