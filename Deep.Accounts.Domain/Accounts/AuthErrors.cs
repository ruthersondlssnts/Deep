using Deep.Common.Domain;

namespace Deep.Accounts.Domain.Accounts;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = Error.Authorization(
        "Auth.InvalidCredentials",
        "Invalid email or password"
    );

    public static readonly Error AccountInactive = Error.Authorization(
        "Auth.AccountInactive",
        "Account is inactive"
    );

    public static readonly Error InvalidRefreshToken = Error.Authorization(
        "Auth.InvalidRefreshToken",
        "Invalid or expired refresh token"
    );

    public static readonly Error EmailAlreadyExists = Error.Conflict(
        "Auth.EmailAlreadyExists",
        "An account with this email already exists"
    );
}
