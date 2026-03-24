using Vast.Common.Domain.Auditing;

namespace Vast.Common.Application.Auditing;

public sealed class Audit
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public AuditType AuditType { get; private set; }
    public string TableName { get; private set; } = string.Empty;
    public string PrimaryKey { get; private set; } = string.Empty;
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string? ChangedColumns { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    private Audit() { }

    public static Audit Create(
        Guid? userId,
        AuditType auditType,
        string tableName,
        string primaryKey,
        string? oldValues,
        string? newValues,
        string? changedColumns
    ) =>
        new Audit
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            AuditType = auditType,
            TableName = tableName,
            PrimaryKey = primaryKey,
            OldValues = oldValues,
            NewValues = newValues,
            ChangedColumns = changedColumns,
            OccurredAtUtc = DateTime.UtcNow,
        };
}
