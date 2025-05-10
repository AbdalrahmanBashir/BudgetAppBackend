using MediatR;

namespace BudgetAppBackend.Application.Features.AI.AskAI
{
    public class AskChatGPTQuery : IRequest<IAsyncEnumerable<string>>
    {
        public string Prompt { get; set; }
        public Guid UserId { get; set; }
    }

}
