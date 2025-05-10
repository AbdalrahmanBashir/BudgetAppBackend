using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.TransactionAggregate;
using BudgetAppBackend.Domain.TransactionAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.Features.Transactions.UpdateTransaction
{
    public class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand, Unit>
    {
        private readonly ITransactionRepository _transactionRepository;    
        public UpdateTransactionCommandHandler(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }
        public async Task<Unit> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
        {
            var transactionId = TransactionId.Create(request.UpdateTransactionDto.TransactionId);
            var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
            if (transaction is null)
            {
                throw new KeyNotFoundException("Transaction not found.");
            }
            if (transaction.UserId.Id != request.UpdateTransactionDto.UserId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this transaction.");
            }

            if (!Enum.TryParse<TransactionType>(request.UpdateTransactionDto.transactionType, true, out var transactionType))
                throw new ArgumentException($"Invalid transaction type: {request.UpdateTransactionDto.transactionType}");

            var amount = transactionType == TransactionType.Expense ? -Math.Abs(request.UpdateTransactionDto.Amount) : Math.Abs(request.UpdateTransactionDto.Amount);

            transaction.UpdateTransaction(
                amount,
                request.UpdateTransactionDto.TransactionDate,
                request.UpdateTransactionDto.payee,
                request.UpdateTransactionDto.category,
                transactionType
            );
            await _transactionRepository.UpdateAsync(transaction, cancellationToken);
            return Unit.Value;
        }
    }
}
