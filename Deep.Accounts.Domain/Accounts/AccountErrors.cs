using Deep.Common.Domain;

namespace Deep.Accounts.Domain.Accounts;

public static class AccountErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Account.NotFound", $"The user with the identifier {id} was not found");

    public static readonly Error AccountAlreadyExists = Error.Problem(
        "Account.AlreadyExists",
        "The user already exists");

    public static readonly Error InvalidRole = Error.Problem(
       "Role.Invalid",
       "The role is invalid");
}
