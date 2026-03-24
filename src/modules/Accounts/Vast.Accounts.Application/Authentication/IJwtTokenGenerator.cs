using Vast.Accounts.Domain.Accounts;

namespace Vast.Accounts.Application.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Account account);
}
