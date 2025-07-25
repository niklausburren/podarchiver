using System.Text.Json.Serialization;

namespace PodArchiver.Models;

/// <summary>
/// Represents the configuration for a single podcast feed.
/// </summary>
public class FeedConfig
{
    #region Properties

    /// <summary>
    /// Gets or sets the title of the feed. If null, the title will be taken from the RSS feed itself.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the feed URL.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum number of episodes to keep. If null, all episodes are kept.
    /// </summary>
    [JsonPropertyName("count")]
    public int? Count { get; set; }

    #endregion
}
