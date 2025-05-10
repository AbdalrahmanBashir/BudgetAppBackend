using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.DTOs.BudgetDTOs;
using BudgetAppBackend.Application.DTOs.Reports;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using BudgetAppBackend.Domain.TransactionAggregate;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.Features.Reports.GetFinancialReport
{
    public class GetFinancialReportQueryHandler : IRequestHandler<GetFinancialReportQuery, byte[]>
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly IPlaidTransactionRepository _plaidTransactionRepo;
        private readonly IBudgetRepository _budgetRepo;
        private readonly IPdfReportService _pdfService;
        private readonly IAuthRepository _authRepository;

        public GetFinancialReportQueryHandler(
            ITransactionRepository transactionRepo,
            IPlaidTransactionRepository plaidTransactionRepo,
            IBudgetRepository budgetRepo,
            IPdfReportService pdfService,
            IAuthRepository authRepository)
        {
            _transactionRepo = transactionRepo;
            _plaidTransactionRepo = plaidTransactionRepo;
            _budgetRepo = budgetRepo;
            _pdfService = pdfService;
            _authRepository = authRepository;
        }
        public async Task<byte[]> Handle(GetFinancialReportQuery request, CancellationToken cancellationToken)
        {

            // Calculate 30-day window
            var startDate = DateTime.UtcNow.AddDays(-30);

            // Fetch transactions
            var transactions = await _transactionRepo.GetTransactionsByUserIdAndDateRangeAsync(request.UserId, startDate, cancellationToken);

            var plaidTransactions = await _plaidTransactionRepo.GetTransactionsByUserIdAndDateRangeAsync(request.UserId, startDate, cancellationToken);

            // Combine transactions and order by date  
            var allTransactions = transactions.Concat(plaidTransactions);

            // Fetch budgets
            var budgets = await _budgetRepo.GetActiveBudgetsAsync(request.UserId, cancellationToken);

            // income
            var plaidIncome = await _plaidTransactionRepo.GetTotalIncomeDateRangeAsync(request.UserId, startDate, cancellationToken);
            var plaidExpenses = await _plaidTransactionRepo.GetTotalExpensesDateRangeAsync(request.UserId, startDate, cancellationToken);

            var manualIncome = await _transactionRepo.GetTotalIncomeDateRangeAsync(request.UserId, startDate, cancellationToken);
            var manualExpenses = await _transactionRepo.GetTotalExpensesDateRangeAsync(request.UserId, startDate, cancellationToken);

            // Combine income and expenses
            var income = plaidIncome + manualIncome;
            var expenses = plaidExpenses + manualExpenses;

           
            // Get User Name and email.
            var user = await _authRepository.GetUserByIdAsync(request.UserId, cancellationToken);

            // Generate PDF
            var reportData = new FinancialReportDto
            {
                UserFullName = user?.FirstName + " " + user.LastName,
                ReportDate = DateTime.UtcNow,
                ReportId = Guid.NewGuid(),
                ReportPeriod = DateTime.UtcNow.AddDays(-30).ToString("MMMM dd, yyyy") + " - " + DateTime.UtcNow.ToString("MMMM dd, yyyy"),
                TotalIncome = income,
                TotalExpenses = expenses,

                Budgets = budgets.Select(b => new BudgetDto
                {
                    Title = b.Title,
                    TotalAmount = b.TotalAmount,
                    SpentAmount = b.SpendAmount,
                    Category = b.Category
                }).ToList(),
                RecentTransactions = allTransactions.OrderByDescending(t => t.TransactionDate)
                .Select(t =>
                    new TransactionDto(
                        t.TransactionId,
                        t.TransactionDate,
                        t.Amount,
                        t.Payee,
                        t.Categories
                    )
                ).ToList(),

            };

            return _pdfService.GenerateFinancialReport(reportData);
        }
    }
}
