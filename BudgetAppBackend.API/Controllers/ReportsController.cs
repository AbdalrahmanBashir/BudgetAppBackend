using BudgetAppBackend.Application.Features.Reports.GetFinancialReport;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BudgetAppBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly ISender _sender;

        public ReportsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> GenerateMonthlyReport([FromQuery] Guid userId)
        {
            var query = new GetFinancialReportQuery
            {
                UserId = UserId.Create(userId)
            };

            var pdfBytes = await _sender.Send(query);
            return File(pdfBytes, "application/pdf", "30DayReport.pdf");
        }
    }
}
