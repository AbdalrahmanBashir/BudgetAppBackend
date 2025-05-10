using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BudgetAppBackend.Application.Configuration;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using BudgetAppBackend.Application.Service;
using BudgetAppBackend.Domain.BudgetAggregate;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class OllamaAIChatService : IAIChatService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaSettings _ollamaSettings;
    private readonly ILogger<OllamaAIChatService> _logger;

    public OllamaAIChatService(
        HttpClient httpClient,
        IOptions<OllamaSettings> ollamaSettings,
        ILogger<OllamaAIChatService> logger)
    {
        _httpClient = httpClient;
        _ollamaSettings = ollamaSettings.Value;
        _logger = logger;
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(string prompt, IEnumerable<TransactionDto> transactions, List<Budget> budgetDtos)
    {
        var fullPrompt = BuildPrompt(prompt, transactions, budgetDtos);
        _logger.LogInformation("Sending prompt to Ollama AI: {Prompt}", fullPrompt);

        var ollamaEndpoint = _ollamaSettings.Endpoint;
        float temperature = 0.0f;
        int maxTokens = 10;

        using var response = await _httpClient.SendAsync(
        new HttpRequestMessage(HttpMethod.Post, $"{ollamaEndpoint}/api/generate")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    model = _ollamaSettings.Model,
                    prompt = fullPrompt,
                    stream = true,
                    temperature,
                    max_tokens = maxTokens
                }),
                Encoding.UTF8,
                "application/json"
            ),
            Headers = { { "Accept", "text/event-stream" } }
        },
          HttpCompletionOption.ResponseHeadersRead
        );

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Ollama API request failed with status: {StatusCode}", response.StatusCode);
            yield return $"\n[Error: API request failed ({response.StatusCode})]";
            yield break;
        }

        var responseStream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(
            responseStream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1,
            leaveOpen: true
        );

        var buffer = new char[1];
        var jsonBuffer = new StringBuilder();

        while (true)
        {
            var bytesRead = await reader.ReadAsync(buffer, 0, 1);
            if (bytesRead == 0) break;

            jsonBuffer.Append(buffer[0]);

            if (buffer[0] == '\n')
            {
                var line = jsonBuffer.ToString();
                jsonBuffer.Clear();

                if (string.IsNullOrWhiteSpace(line)) continue;

                using var jsonDoc = JsonDocument.Parse(line);
                if (jsonDoc.RootElement.TryGetProperty("response", out var responseProperty) &&
                    responseProperty.GetString() is { } chunk &&
                    !string.IsNullOrEmpty(chunk))
                {
                    yield return chunk;
                }
            }
        }
    }

    private string BuildPrompt(string prompt, IEnumerable<TransactionDto> transactions, List<Budget> budgets)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Analyze these financial details and answer the question:");

        builder.AppendLine("\n[Transactions]");
        foreach (var t in transactions)
        {
            builder.AppendLine($"- {t.TransactionDate:yyyy-MM-dd}: {t.Payee} " +
                             $"{t.Amount:C} ({t.Categories})");
        }

        builder.AppendLine("\n[Budgets]");
        foreach (var b in budgets)
        {
            builder.AppendLine($"- {b.Category}: " +
                             $"Spent {b.SpendAmount:C} of {b.TotalAmount:C} " +
                             $"({b.TotalAmount - b.SpendAmount:C} remaining)");
        }

        builder.AppendLine($"\n[Question]\n{prompt}");
        builder.AppendLine("\n[Instructions]\nProvide a detailed analysis with specific recommendations.");

        return builder.ToString();
    }

    // Add this nested class for response parsing
    private class OllamaResponse
    {
        public string Response { get; set; } = string.Empty;
    }
}