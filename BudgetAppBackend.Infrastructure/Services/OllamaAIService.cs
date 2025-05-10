using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BudgetAppBackend.Application.Configuration;
using BudgetAppBackend.Application.DTOs.AiAnalysisDTOS;
using BudgetAppBackend.Application.DTOs.TransactionDTOs;
using BudgetAppBackend.Application.Service;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BudgetAppBackend.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the AI analysis service using the Ollama LLM API.
    /// This service generates financial insights based on user transactions and budgets.
    /// </summary>
    public class OllamaAIService : IAIAnalysisService
    {

        private readonly HttpClient _httpClient;
        //private readonly OllamaSettings _ollamaSettings;
        private readonly Geminisetting _geminiSettings;

        public OllamaAIService(HttpClient httpClient, IOptions<Geminisetting> geminisetting)
        {
            _httpClient = httpClient;
            //_ollamaSettings = ollamaSettings.Value;
            _geminiSettings = geminisetting.Value;
        }

        /// <summary>
        /// Analyzes the last 3 months of user transactions using an LLM and returns a comprehensive financial report.
        /// </summary>
        /// <param name="transactions">User transaction history from the last 3 months.</param>
        /// <returns>A structured financial analysis object including trends, risks, and suggestions.</returns>
        public async Task<QuarterlyTransactionAnalysis> AnalyzeSpendingPatternsForLastThreeMonth(IEnumerable<TransactionDto> transactions)
        {
            DateTime threeMonthsAgo = DateTime.UtcNow.AddDays(-90);
            var promptBuilder = new StringBuilder();

            // Build prompt for LLM analysis
            promptBuilder.AppendLine("You are a highly skilled financial data analyst with expertise in transaction monitoring and anomaly detection.");
            promptBuilder.AppendLine("IMPORTANT: You must respond with ONLY valid JSON matching the structure specified below. Do not include any other text or markdown formatting.");
            promptBuilder.AppendLine("The response must be a single, complete JSON object with no trailing text.");
            promptBuilder.AppendLine("Do not include arrays for anomaliesOrRedFlags and recommendations - they should be string fields.");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Provide a detailed, comprehensive analysis of the transactions from the last 3 months (from" + threeMonthsAgo.ToString() + " to" + DateTime.Now.ToString() + ". For each section, provide extensive analysis with specific examples and detailed explanations:");

            // Split transactions into income and expenses
            var expenseTransactions = transactions.Where(t => t.Amount < 0).ToList();
            var incomeTransactions = transactions.Where(t => t.Amount > 0).ToList();

            // Calculate financial metrics
            decimal totalExpenses = expenseTransactions.Sum(t => -t.Amount);
            decimal totalIncome = incomeTransactions.Sum(t => t.Amount);
            decimal netSavings = totalIncome - totalExpenses;


            // Append transaction summaries
            var totalSpending = transactions.Sum(t => Math.Abs(t.Amount));
            var averageTransaction = transactions.Average(t => Math.Abs(t.Amount));
            var maxTransaction = transactions.Max(t => Math.Abs(t.Amount));
            var minTransaction = transactions.Min(t => Math.Abs(t.Amount));
            var transactionCount = transactions.Count();


            // Transaction summary
            promptBuilder.AppendLine("\nLast 3 Months Financial Summary:");
            promptBuilder.AppendLine($"- Total Income: ${totalIncome:F2} ({incomeTransactions.Count} transactions)");
            promptBuilder.AppendLine($"- Total Expenses: ${totalExpenses:F2} ({expenseTransactions.Count} transactions)");
            promptBuilder.AppendLine($"- Net Savings: ${netSavings:F2}");

            // Category breakdowns
            AddCategoryAnalysis(promptBuilder, incomeTransactions, "Income", totalIncome, false);
            AddCategoryAnalysis(promptBuilder, expenseTransactions, "Expense", totalExpenses, true);

            // Daily patterns
            AddDailyPatterns(promptBuilder, incomeTransactions, "Income");
            AddDailyPatterns(promptBuilder, expenseTransactions, "Expense");

            // Detailed transactions
            promptBuilder.AppendLine("\nDetailed Transactions:");
            foreach (var tx in transactions.OrderByDescending(t => t.TransactionDate))
            {
                string type = tx.Amount < 0 ? "Expense" : "Income";
                promptBuilder.AppendLine($"- {tx.TransactionDate:yyyy-MM-dd}: {type} ${Math.Abs(tx.Amount):F2} " +
                    $"[{tx.Categories.FirstOrDefault()}] {tx.Payee}");
            }

            promptBuilder.AppendLine("\nRequired JSON Response Structure (EXACTLY AS SHOWING DO NOT CHANGE PROPERTY NAMES EVEN DO NOT ADD ONE LETTER EXTRA):");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"overview\": \"A comprehensive summary of spending patterns (minimum 300 words)\",");
            promptBuilder.AppendLine("  \"spendingTrends\": \"Detailed analysis of spending trends and patterns (minimum 300 words)\",");
            promptBuilder.AppendLine("  \"categoryAnalysis\": \"In-depth analysis of spending by category with specific examples (minimum 300 words)\",");
            promptBuilder.AppendLine("  \"anomaliesOrRedFlags\": \"Detailed identification of unusual transactions or patterns with explanations (minimum 200 words)\",");
            promptBuilder.AppendLine("  \"timeBasedInsights\": \"Comprehensive analysis of spending patterns over time with trend identification (minimum 300 words)\",");
            promptBuilder.AppendLine("  \"recommendations\": \"Detailed, actionable recommendations for improvement with specific examples (minimum 300 words)\",");
            promptBuilder.AppendLine("  \"riskAssessment\": \"Thorough assessment of financial risks and concerns with mitigation strategies (minimum 200 words)\",");
            promptBuilder.AppendLine("  \"opportunities\": \"Detailed analysis of potential opportunities for optimization (minimum 200 words)\",");
            promptBuilder.AppendLine("  \"futureProjections\": \"Analysis of future spending patterns and potential impacts (minimum 200 words)\",");
            promptBuilder.AppendLine("  \"comparisonAnalysis\": \"Comparison with typical spending patterns and benchmarks (minimum 200 words)\",");
            promptBuilder.AppendLine("  \"disclaimer\": \"This analysis is for informational purposes only and not financial advice\",");
            promptBuilder.AppendLine("}");

            promptBuilder.AppendLine("\nIMPORTANT: For each section, provide detailed analysis with specific examples, data points, and actionable insights. " +
                "Use the transaction data provided to support your analysis. and please do not change the property names form overview to comparisonAnalysis");

            var prompt = promptBuilder.ToString();
            var payload = GeneratePayload(prompt);
            var geminiEndpoint = $"{_geminiSettings.EndPoints}?key={_geminiSettings.GeminiApiKey}";


            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage responseContent = await _httpClient.PostAsync(geminiEndpoint, new StringContent(payload, Encoding.UTF8, "application/json"));

            try
            {
                using var document = JsonDocument.Parse(await responseContent.Content.ReadAsStreamAsync());
                var root = document.RootElement;
                Console.WriteLine($"Response root: {root}");

                var llmResponse = root
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
                if (string.IsNullOrEmpty(llmResponse))
                {
                    throw new InvalidOperationException("Empty response received from LLM");
                }

                // Strip possible formatting artifacts
                //llmResponse = Regex.Replace(llmResponse, @"<think>.*?</think>", "", RegexOptions.Singleline);

                Console.WriteLine($"Raw LLM Response: {llmResponse}");

                // Extract clean JSON object from raw string
                var jsonStart = llmResponse.IndexOf('{');
                if (jsonStart == -1)
                {
                    throw new InvalidOperationException("No JSON object found in response");
                }

                var stack = new Stack<int>();
                var jsonEnd = -1;

                for (int i = jsonStart; i < llmResponse.Length; i++)
                {
                    if (llmResponse[i] == '{')
                    {
                        stack.Push(i);
                    }
                    else if (llmResponse[i] == '}')
                    {
                        if (stack.Count > 0)
                        {
                            stack.Pop();
                            if (stack.Count == 0)
                            {
                                jsonEnd = i;
                                break;
                            }
                        }
                    }
                }

                if (jsonEnd == -1 || jsonEnd <= jsonStart)
                {
                    throw new InvalidOperationException("No complete JSON object found in response");
                }

                var jsonString = llmResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);

                jsonString = jsonString
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Replace("\n", " ")
                    .Replace("\r", " ")
                    .Replace("\\\"", "\"")
                    .Replace("Document:", "")
                    .Replace("01.json", "")
                    .Trim();

                // Remove any nested JSON objects or arrays for the required fields
                var tempDoc = JsonDocument.Parse(jsonString);
                var tempRoot = tempDoc.RootElement;

                var cleanedObject = new
                {
                    overview = GetFlexibleProperty(tempRoot, "overview"),
                    spendingTrends = GetFlexibleProperty(tempRoot, "spendingTrends"),
                    categoryAnalysis = GetFlexibleProperty(tempRoot, "categoryAnalysis"),
                    anomaliesOrRedFlags = GetFlexibleProperty(tempRoot, "anomaliesOrRedFlags"),
                    timeBasedInsights = GetFlexibleProperty(tempRoot, "timeBasedInsights"),
                    recommendations = GetFlexibleProperty(tempRoot, "recommendations"),
                    riskAssessment = GetFlexibleProperty(tempRoot, "riskAssessment"),
                    opportunities = GetFlexibleProperty(tempRoot, "opportunities"),
                    futureProjections = GetFlexibleProperty(tempRoot, "futureProjections"),
                    comparativeAnalysis = GetFlexibleProperty(tempRoot, "comparisonAnalysis", "comparativeAnalysis"),
                    disclaimer = GetFlexibleProperty(tempRoot, "disclaimer")
                };

                // Convert back to JSON string
                jsonString = System.Text.Json.JsonSerializer.Serialize(cleanedObject);

                Console.WriteLine($"Cleaned JSON string: {jsonString}");

                // Re-parse and map to DTO
                using var analysisDocument = JsonDocument.Parse(jsonString);
                var analysisRoot = analysisDocument.RootElement;

                return new QuarterlyTransactionAnalysis(
                    Overview: GetFlexibleProperty(analysisRoot, "overview") ?? "No overview provided",
                    SpendingTrends: GetFlexibleProperty(analysisRoot, "spendingTrends") ?? "No spending trends identified",
                    CategoryAnalysis: GetFlexibleProperty(analysisRoot, "categoryAnalysis") ?? "No category analysis available",
                    AnomaliesOrRedFlags: GetFlexibleProperty(analysisRoot, "anomaliesOrRedFlags") ?? "No anomalies detected",
                    TimeBasedInsights: GetFlexibleProperty(analysisRoot, "timeBasedInsights") ?? "No time-based insights available",
                    Recommendations: GetFlexibleProperty(analysisRoot, "recommendations") ?? "No recommendations provided",
                    RiskAssessment: GetFlexibleProperty(analysisRoot, "riskAssessment") ?? "No risk assessment available",
                    Opportunities: GetFlexibleProperty(analysisRoot, "opportunities") ?? "No opportunities identified",
                    FutureProjections: GetFlexibleProperty(analysisRoot, "futureProjections") ?? "No future projections available",
                    ComparativeAnalysis: GetFlexibleProperty(analysisRoot, "comparativeAnalysis", "comparisonAnalysis")
                        ?? "No comparative analysis available",
                    Disclaimer: GetFlexibleProperty(analysisRoot, "disclaimer")
                        ?? "This analysis is for informational purposes only and not financial advice"
                );
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse JSON. Response content: {responseContent}", ex);
            }
        }

        private void AddCategoryAnalysis(StringBuilder pb, IEnumerable<TransactionDto> transactions,
                                        string type, decimal total, bool isExpense)
        {
            var breakdown = transactions
                .GroupBy(t => t.Categories.FirstOrDefault() ?? "Uncategorized")
                .Select(g => new {
                    Category = g.Key,
                    Total = isExpense ? g.Sum(t => -t.Amount) : g.Sum(t => t.Amount),
                    Count = g.Count(),
                    Percentage = total > 0 ? (g.Sum(t => Math.Abs(t.Amount)) / total * 100) : 0
                })
                .OrderByDescending(x => x.Total);

            pb.AppendLine($"\n{type} Category Analysis:");
            foreach (var cat in breakdown)
            {
                pb.AppendLine($"- {cat.Category}: ${cat.Total:F2} ({cat.Count} transactions, {cat.Percentage:F1}%)");
            }
        }

        private void AddDailyPatterns(StringBuilder pb, IEnumerable<TransactionDto> transactions, string type)
        {
            var daily = transactions
                .GroupBy(t => t.TransactionDate.Date)
                .Select(g => new {
                    Date = g.Key,
                    Total = type == "Expense" ? g.Sum(t => -t.Amount) : g.Sum(t => t.Amount)
                })
                .OrderBy(d => d.Date);

            pb.AppendLine($"\nDaily {type} Pattern:");
            foreach (var day in daily)
            {
                pb.AppendLine($"- {day.Date:yyyy-MM-dd}: ${day.Total:F2}");
            }
        }

        private static string GetFlexibleProperty(JsonElement element, params string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                if (element.TryGetProperty(name, out var prop))
                {
                    return prop.GetString()?.Trim() ?? string.Empty;
                }
            }
            return string.Empty;
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
    }

}
