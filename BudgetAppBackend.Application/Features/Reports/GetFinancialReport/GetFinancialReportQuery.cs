using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;

namespace BudgetAppBackend.Application.Features.Reports.GetFinancialReport
{
    public class GetFinancialReportQuery : IRequest<byte[]>
    {
        public UserId UserId { get; set; }
    }
}
