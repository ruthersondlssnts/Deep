using System.Text.Json;
using Deep.Common.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Deep.Common.Application.Outbox;

public sealed class InsertOutboxMessagesInterceptor(Type dbContextType) : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null || !dbContextType.IsInstanceOfType(eventData.Context))
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        InsertOutboxMessages(eventData.Context);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void InsertOutboxMessages(DbContext context)
    {
        List<OutboxMessage> outboxMessages = context
            .ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                IReadOnlyCollection<IDomainEvent> domainEvents = entity.GetDomainEvents();
                entity.ClearDomainEvents();
                return domainEvents;
            })
            .Select(domainEvent => new OutboxMessage
            {
                Id = domainEvent.Id,
                Type = domainEvent.GetType().Name,
                Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonSerializerOptions),
                OccurredAtUtc = domainEvent.OccurredAtUtc,
                ProcessedAtUtc = null,
                Error = null
            })
            .ToList();

        if (outboxMessages.Count == 0)
        {
            return;
        }

        context.Set<OutboxMessage>().AddRange(outboxMessages);
    }
}
