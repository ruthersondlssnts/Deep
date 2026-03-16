using System.Text.Json;
using Deep.Common.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Deep.Common.Application.Outbox;

public abstract class InsertOutboxMessagesInterceptorBase(IOutboxNotifier outboxNotifier)
    : SaveChangesInterceptor
{
    private readonly IOutboxNotifier _outboxNotifier = outboxNotifier;

    private bool _hasMessages;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is not null)
        {
            _hasMessages = InsertOutboxMessages(eventData.Context);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default
    )
    {
        if (_hasMessages)
        {
            _outboxNotifier.Notify();
        }

        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private static bool InsertOutboxMessages(DbContext context)
    {
        var outboxMessages = context
            .ChangeTracker.Entries<Entity>()
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
                Content = JsonSerializer.Serialize(
                    domainEvent,
                    domainEvent.GetType(),
                    JsonSerializerOptions
                ),
                OccurredAtUtc = domainEvent.OccurredAtUtc,
                ProcessedAtUtc = null,
                Error = null,
            })
            .ToList();

        if (outboxMessages.Count == 0)
        {
            return false;
        }

        context.Set<OutboxMessage>().AddRange(outboxMessages);
        return true;
    }
}
