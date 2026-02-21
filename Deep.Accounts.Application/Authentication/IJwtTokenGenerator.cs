using Deep.Accounts.Domain.Accounts;

namespace Deep.Accounts.Application.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Account account);
}
