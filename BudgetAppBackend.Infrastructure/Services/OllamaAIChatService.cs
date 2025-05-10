using System.Text;
using System.Text.Json;
using BudgetAppBackend.Application.Configuration;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using BudgetAppBackend.Application.Service;
using BudgetAppBackend.Domain.BudgetAggregate;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

public class OllamaAIChatService : IAIChatService
{
    private readonly HttpClient _httpClient;
    private readonly Geminisetting _geminiSettings;
    private readonly ILogger<OllamaAIChatService> _logger;
    private readonly string[] _financialKeywords = new[] {
        "budget", "spending", "expense", "income", "savings",
        "transaction", "category", "cashflow", "financial",
        "money", "cost", "price", "amount", "balance", "account"
    };

    public OllamaAIChatService(
        HttpClient httpClient,
        IOptions<Geminisetting> geminisetting,
        ILogger<OllamaAIChatService> logger)
    {
        _httpClient = httpClient;
        _geminiSettings = geminisetting.Value;
        _logger = logger;
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(string prompt, IEnumerable<TransactionDto> transactions, List<Budget> budgetDtos)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            yield return "\n[Error: Prompt cannot be empty]";
            yield break;
        }
        if (!IsFinancialQuery(prompt))
        {
            var detectedKeywords = _financialKeywords.Where(k => prompt.ToLower().Contains(k)).ToList();
            var ss = "I can only assist with financial and budgeting-related questions. Please ask me about your transactions, budgets, spending patterns, or financial analysis.";

            if (detectedKeywords.Any())
            {
                ss += $"\n\nI noticed you mentioned: {string.Join(", ", detectedKeywords)}. Please rephrase your question to focus on these financial aspects.";
            }

            yield return ss;
            yield break;
        }
        var fullPrompt = BuildPrompt(prompt, transactions, budgetDtos);
        _logger.LogInformation("Sending prompt to Gemini AI: {Prompt}", fullPrompt);

        var geminiEndpoint = $"{_geminiSettings.stream}?key={_geminiSettings.GeminiApiKey}";
        var payload = GeneratePayload(fullPrompt);

        using var request = new HttpRequestMessage(HttpMethod.Post, geminiEndpoint)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("API request failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
            yield return $"\n[Error: API request failed ({response.StatusCode})]";
            yield break;
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        var buffer = new char[1024];
        var jsonBuffer = new StringBuilder();
        var inObject = false;
        var braceCount = 0;

        while (!reader.EndOfStream)
        {
            var bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length);
            for (int i = 0; i < bytesRead; i++)
            {
                var c = buffer[i];

                if (c == '{')
                {
                    braceCount++;
                    inObject = true;
                }
                else if (c == '}')
                {
                    braceCount--;
                }

                if (inObject) jsonBuffer.Append(c);

                if (inObject && braceCount == 0)
                {
                    inObject = false;
                    var jsonContent = jsonBuffer.ToString();
                    jsonBuffer.Clear();

                    if (!string.IsNullOrWhiteSpace(jsonContent))
                    {
                        var chunk = ParseJsonChunk(jsonContent);
                        if (!string.IsNullOrEmpty(chunk))
                        {
                            yield return chunk;
                        }
                    }
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

    private static string GeneratePayload(string text)
    {
        var payload = new
        {
            contents = new[]
            {
            new
            {
                parts = new[]
                {
                    new { text = text }
                },
                role = "user"
            }
        },
            generation_config = new
            {
                temperature = 0.4,
                
                top_p = 1,
                top_k = 32,
                max_output_tokens = 2048
            }
        };
        return JsonConvert.SerializeObject(payload);
    }

    private string? ParseJsonChunk(string jsonContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            // Handle error responses
            if (root.TryGetProperty("error", out var errorElement))
            {
                var errorMessage = errorElement.GetProperty("message").GetString();
                _logger.LogError("Gemini API error: {Error}", errorMessage);
                return $"\n[API Error: {errorMessage}]";
            }

            // Extract text content
            return root
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError("JSON parsing error: {Error}", ex.Message);

            return "\n[Data parsing error]";
        }
    }

    private bool IsFinancialQuery(string prompt)
    {
        var lowerPrompt = prompt.ToLower();
        return _financialKeywords.Any(keyword => lowerPrompt.Contains(keyword));
    }



}