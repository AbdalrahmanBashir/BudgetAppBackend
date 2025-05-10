using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.DTOs.AiAnalysisDTOS;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using BudgetAppBackend.Application.Service;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.Features.AI.GetQuarterlyTransactionAnalysis
{
    public class GetQuarterlyTransactionAnalysisQueryHandler : IRequestHandler<GetQuarterlyTransactionAnalysisQuery, QuarterlyTransactionAnalysis>
    {
        private readonly IAIAnalysisService _aiService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPlaidTransactionRepository _plaidTransactionRepository;

        public GetQuarterlyTransactionAnalysisQueryHandler(
            IAIAnalysisService aiService,
            ITransactionRepository transactionRepository,
            IPlaidTransactionRepository plaidTransactionRepository)
        {
            _aiService = aiService;
            _transactionRepository = transactionRepository;
            _plaidTransactionRepository = plaidTransactionRepository;
        }

        public async Task<QuarterlyTransactionAnalysis> Handle(GetQuarterlyTransactionAnalysisQuery request, CancellationToken cancellationToken)
        {
            var userId = UserId.Create(request.UserId);

            var manualTransactions = (await _transactionRepository.GetThreeMonthTransactionsByUserIdAsync(userId)) ?? Enumerable.Empty<TransactionDto>();
            var plaidTransactions = (await _plaidTransactionRepository.GetThreeMonthTransactionsByUserIdAsync(userId)) ?? Enumerable.Empty<TransactionDto>();

            var transactions = manualTransactions.Concat(plaidTransactions);
            if (!transactions.Any())
            {
                throw new KeyNotFoundException("No transactions to analyze");
            }

            var quarterlyTransactionAnalysis = await _aiService.AnalyzeSpendingPatternsForLastThreeMonth(plaidTransactions);

            return quarterlyTransactionAnalysis;
        }
    }
}
