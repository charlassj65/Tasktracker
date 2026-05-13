using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TaskTracker.Application.Configuration;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Infrastructure.AI;

/// <summary>
/// Calls the Anthropic Messages API to produce a natural-language task summary.
/// Activated automatically when AiProvider:ApiKey is set in configuration.
/// Falls back to a structured prompt if the API call fails.
/// </summary>
public class ClaudeAiSummaryProvider : IAiSummaryProvider
{
    private readonly HttpClient _httpClient;
    private readonly AiProviderSettings _settings;
    private readonly ILogger<ClaudeAiSummaryProvider> _logger;

    private const string AnthropicApiVersion = "2023-06-01";
    private const string MessagesEndpoint = "https://api.anthropic.com/v1/messages";

    public ClaudeAiSummaryProvider(
        HttpClient httpClient,
        AiProviderSettings settings,
        ILogger<ClaudeAiSummaryProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;
    }

    public async Task<string> SummarizeTasksAsync(IEnumerable<TaskItem> tasks)
    {
        var taskList = tasks.ToList();

        var prompt = BuildPrompt(taskList);

        try
        {
            var requestBody = new
            {
                model = _settings.Model,
                max_tokens = 256,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, MessagesEndpoint);
            request.Headers.Add("x-api-key", _settings.ApiKey);
            request.Headers.Add("anthropic-version", AnthropicApiVersion);
            request.Content = JsonContent.Create(requestBody);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadFromJsonAsync<JsonDocument>();

            var summary = responseBody?
                .RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            return summary ?? FallbackSummary(taskList);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Claude API call failed, falling back to local summary");
            return FallbackSummary(taskList);
        }
    }

    private static string BuildPrompt(IReadOnlyList<TaskItem> tasks)
    {
        var total = tasks.Count;
        var completed = tasks.Count(t => t.Status == TaskItemStatus.Done);
        var inProgress = tasks.Count(t => t.Status == TaskItemStatus.InProgress);
        var pending = tasks.Count(t => t.Status == TaskItemStatus.Todo);

        var taskLines = string.Join("\n", tasks.Select(t =>
            $"- [{t.Status}] {t.Title}" +
            (t.DueDate.HasValue ? $" (due {t.DueDate.Value:yyyy-MM-dd})" : string.Empty)));

        return
            $"Summarize the following task list in one concise, encouraging sentence. " +
            $"Total: {total}, Completed: {completed}, In Progress: {inProgress}, Pending: {pending}.\n\n" +
            $"Tasks:\n{taskLines}\n\n" +
            $"Reply with only the summary sentence — no preamble, no extra text.";
    }

    private static string FallbackSummary(IReadOnlyList<TaskItem> tasks)
    {
        var completed = tasks.Count(t => t.Status == TaskItemStatus.Done);
        var inProgress = tasks.Count(t => t.Status == TaskItemStatus.InProgress);
        var pending = tasks.Count(t => t.Status == TaskItemStatus.Todo);

        return $"You have {tasks.Count} tasks today. " +
               $"{completed} completed, {inProgress} in progress, {pending} pending.";
    }
}
