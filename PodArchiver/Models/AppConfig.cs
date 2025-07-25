using System.Text.Json.Serialization;

namespace PodArchiver.Models;

/// <summary>
/// Represents the application configuration, including output path and feed list.
/// </summary>
public class AppConfig
{
    #region Properties

    /// <summary>
    /// Gets or sets the output path where episodes will be saved.
    /// </summary>
    [JsonPropertyName("outputPath")]
    public string OutputPath { get; set; } = "downloads";

    /// <summary>
    /// Gets or sets the list of download times (HH:mm) per day.
    /// </summary>
    [JsonPropertyName("downloadTimes")]
    public List<TimeSpan>? DownloadTimes { get; set; }

    /// <summary>
    /// Gets or sets the list of feed configurations.
    /// </summary>
    [JsonPropertyName("feeds")]
    public List<FeedConfig> Feeds { get; set; } = [];

    #endregion
}
