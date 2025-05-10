using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.TransactionAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.Features.Transactions.DeleteTransaction
{
    public class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, Unit>
    {
        private readonly ITransactionRepository _transactionRepository;
        public DeleteTransactionCommandHandler(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }
        public async Task<Unit> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
        {
            var transactionId = TransactionId.Create(request.DeleteTransactionDto.TransactionId);
           var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
            if (transaction is null)
            {
                throw new KeyNotFoundException("Transaction not found.");
            }
            if (transaction.UserId.Id != request.DeleteTransactionDto.UserId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this transaction.");
            }
            transaction.DeleteTransaction();
            await _transactionRepository.DeleteAsync(transaction, cancellationToken);
            return Unit.Value;
        }
    }
}
