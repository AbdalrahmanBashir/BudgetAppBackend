using BudgetAppBackend.Domain.Commons;

namespace BudgetAppBackend.Domain.Exceptions.BudgetExceptions
{
    public class BudgetInvalidTitleException : DomainException
    {
        public BudgetInvalidTitleException(string? title)
            : base($"Invalid budget title: '{title}'. Title cannot be null, empty, or whitespace.")
        {
        }
    }
}
