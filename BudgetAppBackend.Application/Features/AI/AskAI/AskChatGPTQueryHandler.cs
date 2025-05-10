using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.Service;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.Features.AI.AskAI
{
    public class AskChatGPTQueryHandler : IRequestHandler<AskChatGPTQuery, IAsyncEnumerable<string>>
    {
        private readonly IAIChatService _aiChatService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPlaidTransactionRepository _laidTransactionRepository;
        private readonly IBudgetRepository _budgetRepository;
        public AskChatGPTQueryHandler(IAIChatService aiChatService, IPlaidTransactionRepository laidTransactionRepository,
            IBudgetRepository budgetRepository, 
            ITransactionRepository transactionRepository
            )
        {
            _aiChatService = aiChatService;
            _laidTransactionRepository = laidTransactionRepository;
            _budgetRepository = budgetRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<IAsyncEnumerable<string>> Handle(AskChatGPTQuery request, CancellationToken cancellationToken)
        {
            var userId = UserId.Create(request.UserId);
            // Fetch transactions and budgets
            var manualTransactions = await _transactionRepository.GetAllUserTransactionsAsync(userId, cancellationToken);
            var plaidTransactions = await _laidTransactionRepository.GetAllUserTransactionsAsync(userId, cancellationToken);

            // combine transactions

            var transactions = manualTransactions.Concat(plaidTransactions);
            if (!transactions.Any())
            {
                throw new KeyNotFoundException("No transactions to analyze");
            }


            var budgets = await _budgetRepository.GetActiveBudgetsAsync(userId, cancellationToken);
            if (!budgets.Any())
            {
                throw new KeyNotFoundException("No budgets found for the user");
            }
            // procee straming
            // Call the AI service with the prompt and user data
            return  _aiChatService.StreamMessageAsync(
                request.Prompt,
                transactions,
                budgets
            );

        }
    }
    
}
