using BudgetAppBackend.Domain.Commons;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Quartz;

namespace BudgetAppBackend.Infrastructure.Outbox
{
    [DisallowConcurrentExecution]
    public class OutboxProcessor : IJob
    {
        private readonly ApplicationDbContext _context;
        private readonly IPublisher _publisher;

        public OutboxProcessor(ApplicationDbContext context, IPublisher publisher)
        {
            _context = context;
            _publisher = publisher;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            List<OutboxMessage> messages = await _context
                .Set<OutboxMessage>()
                .Where(m => m.ProcessedAt == null)
                .Take(10)
                .ToListAsync(context.CancellationToken);

            foreach (OutboxMessage outboxMessage in messages)
            {
                IDomainEvent? domainEvent = JsonConvert
                    .DeserializeObject<IDomainEvent>(outboxMessage.Payload,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });
 
                if (domainEvent is null)
                {
                    continue;
                }

               await _publisher.Publish(domainEvent, context.CancellationToken);

                outboxMessage.ProcessedAt = DateTime.UtcNow;
            }
            
            await _context.SaveChangesAsync(context.CancellationToken);
                      
        }
    }
}
