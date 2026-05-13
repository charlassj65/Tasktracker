namespace TaskTracker.Application.Configuration;

public class AiProviderSettings
{
    public const string SectionName = "AiProvider";

    /// <summary>
    /// Provider type to use: "Simple" (default, no API key) or "Claude".
    /// </summary>
    public string Type { get; set; } = "Simple";

    /// <summary>
    /// API key for the selected AI provider. Leave empty to use the local Simple provider.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Model identifier used by real AI providers, e.g. "claude-sonnet-4-6".
    /// </summary>
    public string Model { get; set; } = "claude-sonnet-4-6";
}
