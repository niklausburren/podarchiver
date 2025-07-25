namespace PodArchiver.Models;

/// <summary>
/// Represents a podcast feed with its title, episodes, and optional cover image.
/// </summary>
public class PodcastFeed
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastFeed"/> class.
    /// </summary>
    /// <param name="title">The title of the podcast feed.</param>
    /// <param name="episodes">The list of podcast episodes.</param>
    /// <param name="coverBytes">The cover image as a byte array, or null if not available.</param>
    public PodcastFeed(string title, List<PodcastEpisode> episodes, byte[]? coverBytes)
    {
        this.Title = title;
        this.Episodes = episodes;
        this.CoverBytes = coverBytes;
    }

    #endregion


    #region Properties

    /// <summary>
    /// Gets the title of the podcast feed.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the list of episodes for this podcast feed.
    /// </summary>
    public List<PodcastEpisode> Episodes { get; }

    /// <summary>
    /// Gets the cover image as a byte array, or null if not available.
    /// </summary>
    public byte[]? CoverBytes { get; }

    #endregion
}
