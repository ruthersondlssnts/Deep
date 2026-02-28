using System.Text.Json;
using Deep.Common.Domain.Auditing;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Deep.Common.Application.Auditing;

public sealed class AuditEntry
{
    public EntityEntry Entry { get; }
    public AuditType AuditType { get; set; }
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, object?> PrimaryKey { get; } = [];
    public Dictionary<string, object?> OldValues { get; } = [];
    public Dictionary<string, object?> NewValues { get; } = [];
    public List<string> ChangedColumns { get; } = [];
    public List<PropertyEntry> TemporaryProperties { get; } = [];

    public AuditEntry(EntityEntry entry) => Entry = entry;

    public bool HasTemporaryProperties => TemporaryProperties.Count > 0;

    public Audit ToAudit(Guid? userId)
    {
        foreach (PropertyEntry prop in TemporaryProperties)
        {
            if (prop.Metadata.IsPrimaryKey())
            {
                PrimaryKey[prop.Metadata.Name] = prop.CurrentValue;
            }

            if (AuditType == AuditType.Create)
            {
                NewValues[prop.Metadata.Name] = prop.CurrentValue;
            }
        }

        return Audit.Create(
            userId,
            AuditType,
            TableName,
            JsonSerializer.Serialize(PrimaryKey),
            OldValues.Count > 0 ? JsonSerializer.Serialize(OldValues) : null,
            NewValues.Count > 0 ? JsonSerializer.Serialize(NewValues) : null,
            ChangedColumns.Count > 0 ? JsonSerializer.Serialize(ChangedColumns) : null
        );
    }
}
