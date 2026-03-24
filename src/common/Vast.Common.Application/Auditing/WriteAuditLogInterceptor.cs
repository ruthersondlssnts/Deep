using System.Reflection;
using Vast.Common.Domain.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Vast.Common.Application.Auditing;

public sealed class WriteAuditLogInterceptor(IAuditingUserProvider userProvider)
    : SaveChangesInterceptor
{
    private List<AuditEntry> _pendingAuditEntries = [];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is not null)
        {
            _pendingAuditEntries = CreateAuditEntries(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is not null && _pendingAuditEntries.Count > 0)
        {
            await SaveAuditEntriesAsync(eventData.Context, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private static List<AuditEntry> CreateAuditEntries(DbContext context)
    {
        List<AuditEntry> auditEntries = [];

        foreach (EntityEntry entry in context.ChangeTracker.Entries())
        {
            if (!ShouldAudit(entry))
            {
                continue;
            }

            AuditEntry? auditEntry = CreateAuditEntry(entry);
            if (auditEntry is not null)
            {
                auditEntries.Add(auditEntry);
            }
        }

        return auditEntries;
    }

    private static bool ShouldAudit(EntityEntry entry)
    {
        Type entityType = entry.Entity.GetType();

        if (entityType == typeof(Audit))
        {
            return false;
        }

        if (entry.State is EntityState.Detached or EntityState.Unchanged)
        {
            return false;
        }

        bool hasAuditableAttribute =
            entityType.GetCustomAttribute<AuditableAttribute>() is not null;

        return hasAuditableAttribute;
    }

    private static AuditEntry? CreateAuditEntry(EntityEntry entry)
    {
        Type entityType = entry.Entity.GetType();

        AuditEntry auditEntry = new(entry)
        {
            TableName = entry.Metadata.GetTableName() ?? entityType.Name,
        };

        switch (entry.State)
        {
            case EntityState.Added:
                auditEntry.AuditType = AuditType.Create;
                break;
            case EntityState.Modified:
                auditEntry.AuditType = AuditType.Update;
                break;
            case EntityState.Deleted:
                auditEntry.AuditType = AuditType.Delete;
                break;
            default:
                return null;
        }

        foreach (PropertyEntry property in entry.Properties)
        {
            if (property.Metadata.IsPrimaryKey())
            {
                if (property.IsTemporary)
                {
                    auditEntry.TemporaryProperties.Add(property);
                }
                else
                {
                    auditEntry.PrimaryKey[property.Metadata.Name] = property.CurrentValue;
                }

                if (entry.State == EntityState.Added)
                {
                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                    }
                    else
                    {
                        auditEntry.NewValues[property.Metadata.Name] = property.CurrentValue;
                    }
                }

                continue;
            }

            bool isNotAuditable =
                property.Metadata.PropertyInfo?.GetCustomAttribute<NotAuditableAttribute>()
                is not null;

            if (isNotAuditable)
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                    }
                    else
                    {
                        auditEntry.NewValues[property.Metadata.Name] = property.CurrentValue;
                    }
                    break;

                case EntityState.Deleted:
                    auditEntry.OldValues[property.Metadata.Name] = property.OriginalValue;
                    break;

                case EntityState.Modified:
                    if (
                        property.IsModified
                        && !Equals(property.OriginalValue, property.CurrentValue)
                    )
                    {
                        auditEntry.OldValues[property.Metadata.Name] = property.OriginalValue;
                        auditEntry.NewValues[property.Metadata.Name] = property.CurrentValue;
                        auditEntry.ChangedColumns.Add(property.Metadata.Name);
                    }
                    break;
            }
        }

        if (
            auditEntry.AuditType == AuditType.Update
            && auditEntry.ChangedColumns.Count == 0
            && auditEntry.TemporaryProperties.Count == 0
        )
        {
            return null;
        }

        return auditEntry;
    }

    private async Task SaveAuditEntriesAsync(DbContext context, CancellationToken cancellationToken)
    {
        Guid? userId = userProvider.GetCurrentUserId();

        var audits = _pendingAuditEntries.Select(entry => entry.ToAudit(userId)).ToList();

        _pendingAuditEntries.Clear();

        if (audits.Count == 0)
        {
            return;
        }

        context.Set<Audit>().AddRange(audits);
        await context.SaveChangesAsync(cancellationToken);
    }
}
