using BudgetAppBackend.Application.DTOs.BudgetDTOs;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using BudgetAppBackend.Domain.BudgetAggregate;

namespace BudgetAppBackend.Application.Service
{
    public interface IAIChatService
    {
       
        IAsyncEnumerable<string> StreamMessageAsync(string promt, IEnumerable<TransactionDto> transactions, List<Budget> budgetDtos);
    }
}
