using BudgetAppBackend.Domain.BudgetAggregate;
using BudgetAppBackend.Domain.PlaidTransactionAggregate;
using BudgetAppBackend.Domain.PlaidTransactionAggregate.Entities;
using BudgetAppBackend.Domain.TransactionAggregate;
using BudgetAppBackend.Domain.UserAggregate;
using BudgetAppBackend.Domain.UserAggregate.Entities;
using BudgetAppBackend.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace BudgetAppBackend.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<User> Users { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<PlaidTransaction> PlaidTransactions { get; set; }

        public DbSet<PlaidAccountFingerprint> PlaidAccountFingerprints { get; set; }

        public DbSet<PlaidSyncCursor> PlaidSyncCursors { get; set; }

        public DbSet<Budget> Budgets { get; set; }

    }
}
