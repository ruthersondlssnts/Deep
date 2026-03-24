namespace Vast.Common.Application.Auditing;

public interface IAuditingUserProvider
{
    Guid? GetCurrentUserId();
}
