using BudgetAppBackend.Application.Features.AI.AskAI;
using MediatR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace BudgetAppBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ISender _sender;
        public ChatController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("ask/{userId}")]
        public async Task AskQuestion(
    [FromRoute] Guid userId,
    [FromBody] string question,
    CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HttpContext.Response.WriteAsync("Question cannot be empty.", cancellationToken);
                return;
            }

            try
            {
                var responseStream = await _sender.Send(
                    new AskChatGPTQuery { Prompt = question, UserId = userId },
                    cancellationToken
                );

                // Configure streaming headers
                HttpContext.Response.StatusCode = StatusCodes.Status200OK;
                HttpContext.Response.ContentType = "text/event-stream";
                HttpContext.Response.Headers.Append("Cache-Control", "no-cache");
                HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

                await foreach (var chunk in responseStream.WithCancellation(cancellationToken))
                {
                    // Format as SSE (Server-Sent Events)
                    await HttpContext.Response.WriteAsync($"data: {chunk}\n\n", cancellationToken);
                    await HttpContext.Response.Body.FlushAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                // Send error as final event
                await HttpContext.Response.WriteAsync($"event: error\ndata: {ex.Message}\n\n", cancellationToken);
                await HttpContext.Response.Body.FlushAsync(cancellationToken);
            }
        }
    }
    
}
