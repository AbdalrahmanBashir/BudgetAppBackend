using BudgetAppBackend.Domain.BudgetAggregate.ValueObjects;
using BudgetAppBackend.Domain.Commons;
using BudgetAppBackend.Domain.PlaidTransactionAggregate.ValueObjects;
using BudgetAppBackend.Domain.TransactionAggregate.ValueObjects;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;

namespace BudgetAppBackend.Infrastructure.Outbox
{
    public sealed class DomainEventInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            DbContext? context = eventData.Context;

            if (context != null)
            {
                await ConvertDomainEventsToOutboxMessagesAsync(context, cancellationToken);
            }


            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static async Task ConvertDomainEventsToOutboxMessagesAsync(DbContext context, CancellationToken cancellationToken)
        {
            var aggregates = context.ChangeTracker.Entries()
                         .Where(entry => entry.Entity is AggregateRoot)
                         .Select(entry => entry.Entity as AggregateRoot)
                         .Where(aggregate => aggregate?.DomainEvents.Any() == true)
                         .ToList();


            var outboxMessages = new List<OutboxMessage>();

            foreach (var aggregate in aggregates)
            {
                var aggregateIdProperty = aggregate!.GetType().GetProperty("Id");



                if (aggregateIdProperty == null)
                    throw new InvalidOperationException("Aggregate root must have an 'Id' property.");

                var aggregateIdValue = aggregateIdProperty.GetValue(aggregate);

                Guid aggregateGuid;


                if (aggregateIdValue is UserId userId)
                {

                    aggregateGuid = userId.Id;
                }
                else if (aggregateIdValue is BudgetId budgetId)
                {
                    aggregateGuid = budgetId.Id;
                }
                else if (aggregateIdValue is TransactionId transactionId)
                {
                    aggregateGuid = transactionId.Id;
                }
                else if (aggregateIdValue is PlaidTranId plaidTranId)
                {
                    aggregateGuid = plaidTranId.Id;
                }
                else if (aggregateIdValue is Guid guidId)
                {
                    aggregateGuid = guidId;
                }
                else
                {
                    throw new InvalidOperationException($"Invalid 'Id' type for aggregate {aggregate.GetType().Name}. Expected UserId or Guid.");
                }



                foreach (var domainEvent in aggregate.DomainEvents)
                {
                    outboxMessages.Add(new OutboxMessage(
                        aggregateGuid,
                        domainEvent.GetType().Name,
                        JsonConvert.SerializeObject(domainEvent, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All
                        })
                    ));
                }

                aggregate.ClearDomainEvents();
            }

            await context.Set<OutboxMessage>().AddRangeAsync(outboxMessages, cancellationToken);
        }
    }
}
