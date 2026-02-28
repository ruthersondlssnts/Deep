namespace Deep.Common.Application.Auditing;

public interface IAuditingUserProvider
{
    Guid? GetCurrentUserId();
}
