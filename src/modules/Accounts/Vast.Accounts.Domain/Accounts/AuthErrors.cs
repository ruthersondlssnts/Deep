using Vast.Common.Domain;

namespace Vast.Accounts.Domain.Accounts;

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

    public static readonly Error InvalidResetToken = Error.Authorization(
        "Auth.InvalidResetToken",
        "Invalid or expired password reset token"
    );

    public static readonly Error PasswordRecentlyUsed = Error.Problem(
        "Auth.PasswordRecentlyUsed",
        "Cannot reuse a recently used password"
    );

    public static readonly Error CurrentPasswordIncorrect = Error.Authorization(
        "Auth.CurrentPasswordIncorrect",
        "Current password is incorrect"
    );
}
