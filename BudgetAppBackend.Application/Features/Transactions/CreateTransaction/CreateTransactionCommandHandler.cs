using AutoMapper;
using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.TransactionAggregate;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.Features.Transactions.CreateTransaction
{
    public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Unit>
    {
        private readonly ITransactionRepository _transactionRepository;
        public CreateTransactionCommandHandler(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }
        public async Task<Unit> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
        {
            var dto = request.createTransactionDto;

            var userId = UserId.Create(dto.UserId);

            if (!Enum.TryParse<TransactionType>(dto.transactionType, true, out var transactionType))
                throw new ArgumentException($"Invalid transaction type: {dto.transactionType}");

            var amount = transactionType == TransactionType.Expense ? -Math.Abs(dto.Amount): Math.Abs(dto.Amount);

            var newTransaction = Transaction.Create(
                userId,
                dto.Categories,
                amount,
                dto.TransactionDate,
                dto.payee,
                transactionType
            );

            await _transactionRepository.AddAsync(newTransaction, cancellationToken);

            return Unit.Value;
        }

    }
}
