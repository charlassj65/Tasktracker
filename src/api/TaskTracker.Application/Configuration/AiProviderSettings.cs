namespace TaskTracker.Application.Configuration;

public class AiProviderSettings
{
    public const string SectionName = "AiProvider";

    /// <summary>
    /// Provider type to use: "Simple" (default, no API key) or "External".
    /// </summary>
    public string Type { get; set; } = "Simple";

    /// <summary>
    /// API key for the external AI provider. Leave empty to use the local Simple provider.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Model identifier sent to the external AI provider.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// HTTP endpoint for the external AI provider's messages API.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;
}
