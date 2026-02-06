// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Common.Application.DomainEvents;
using Deep.Common.Application.IntegrationEvents;
using Deep.Transactions.Application.Data;
using Deep.Transactions.Domain.Transaction;
using Deep.Transactions.IntegrationEvents;

namespace Deep.Transactions.Application.Features.Transactions;

internal sealed class TransactionCreatedDomainEventHandler(
    TransactionsDbContext context,
    IEventBus eventBus)
    : DomainEventHandler<TransactionCreatedDomainEvent>
{
    public override async Task Handle(
        TransactionCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var stats = context.Transactions
          .Where(t => t.ProgramId == domainEvent.ProgramId)
          .GroupBy(_ => 1)
          .Select(g => new
          {
              TotalTransactions = g.Count(),
              TotalCustomers = g.Select(x => x.CustomerId).Distinct().Count()
          })
          .FirstOrDefault();

        var totalTransactions = stats?.TotalTransactions ?? 0;
        var totalCustomers = stats?.TotalCustomers ?? 0;

        await eventBus.PublishAsync(
           new TransactionCreatedIntegrationEvent(
               domainEvent.Id,
               domainEvent.OccurredAtUtc,
               totalTransactions,
               totalCustomers,
               domainEvent.ProgramId),
           cancellationToken);
    }
}
