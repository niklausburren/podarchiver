using NLog;
using PodArchiver.Models;
using PodArchiver.Tagging;

namespace PodArchiver.Services;

/// <summary>
/// Downloads podcast episodes from a given RSS feed, saves them to disk, tags them, and optionally cleans up old episodes.
/// </summary>
public class PodArchiver
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PodArchiver"/> class.
    /// </summary>
    /// <param name="feedConfig">The feed configuration containing URL and episode count.</param>
    /// <param name="outputPath">The output path where episodes will be saved.</param>
    /// <param name="token">The cancellation token for cooperative cancellation.</param>
    public PodArchiver(FeedConfig feedConfig, string outputPath, CancellationToken token)
    {
        this.FeedConfig = feedConfig;
        this.OutputPath = outputPath;
        this.Token = token;
    }

    #endregion


    #region Properties

    /// <summary>
    /// Gets the logger instance for logging operations.
    /// </summary>
    private static Logger Logger { get; } = LogManager.GetLogger(nameof(PodArchiver));

    /// <summary>
    /// Gets the RSS feed config.
    /// </summary>
    private FeedConfig FeedConfig { get; }

    /// <summary>
    /// Gets the output path where episodes will be saved.
    /// </summary>
    private string OutputPath { get; }

    /// <summary>
    /// Gets the cancellation token for cooperative cancellation.
    /// </summary>
    private CancellationToken Token { get; }

    /// <summary>
    /// Gets the HTTP client used for downloading.
    /// </summary>
    private static HttpClient Http { get; } = new();

    #endregion


    #region Public Methods

    /// <summary>
    /// Downloads and tags podcast episodes, then cleans up old episodes if necessary.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RunAsync()
    {
        var parser = new RssParser(this.FeedConfig.Url, Http);
        var feed = await parser.ParseAsync();
        var feedTitle = this.FeedConfig.Title ?? feed.Title;

        Logger.Info("Starting archiving of feed: {0}", feedTitle);
        Logger.Info("Feed url: {0}", this.FeedConfig.Url);
        Logger.Info("Max episodes: {0}", this.FeedConfig.Count?.ToString() ?? "all");

        await this.DownloadEpisodesAsync(feed, feedTitle);
        await this.CleanupEpisodesAsync(feed, feedTitle);
    }

    #endregion


    #region Private Methods

    /// <summary>
    /// Downloads and tags all podcast episodes for the given feed.
    /// </summary>
    /// <param name="feed">The podcast feed containing episodes to download.</param>
    /// <param name="feedTitle">The title of the feed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task DownloadEpisodesAsync(PodcastFeed feed, string feedTitle)
    {
        var episodes = this.FeedConfig.Count.HasValue
            ? feed.Episodes.Take(this.FeedConfig.Count.Value)
            : feed.Episodes;

        foreach (var episodeGroup in episodes.GroupBy(e => e.PubDate.Year))
        {
            var albumArtist = episodeGroup.All(e => e.Authors.SequenceEqual(episodeGroup.First().Authors))
                ? episodeGroup.First().Authors
                : ["Various Artists"];

            foreach (var episode in episodeGroup)
            {
                this.Token.ThrowIfCancellationRequested();

                try
                {
                    var albumTitle = $"{feedTitle} ({episodeGroup.Key})";
                    var targetFolder = Path.Combine(this.OutputPath, Utils.Utils.SanitizeFolderName(albumTitle));

                    if (!Directory.Exists(targetFolder))
                    {
                        Logger.Info("Creating directory: {0}", targetFolder);
                        Directory.CreateDirectory(targetFolder);
                    }

                    var fileName = $"{episode.PubDate:yyyy-MM-dd} {Utils.Utils.SanitizeFileName(episode.Title)}{episode.Extension}";
                    var filePath = Path.Combine(targetFolder, fileName);

                    if (File.Exists(filePath))
                    {
                        continue;
                    }

                    Logger.Info("Downloading episode: {0}", filePath);
                    var bytes = await Http.GetByteArrayAsync(episode.Url, this.Token);
                    await File.WriteAllBytesAsync(filePath, bytes, this.Token);

                    Logger.Info("Tagging episode: {0}", filePath);
                    var tagger = new TagWriter(filePath);
                    tagger.ClearAllTags();
                    tagger.WriteTags(episode, albumTitle, feed.CoverBytes, albumArtist);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Logger.Error(ex, "Download episode \"{0}\" failed.", episode.Title);
                }
            }
        }
    }

    /// <summary>
    /// Deletes old episodes if <see cref="FeedConfig.Count"/> is set, keeping only the newest episodes across all year folders.
    /// </summary>
    /// <param name="feed">The podcast feed for which to clean up episodes.</param>
    /// <param name="feedTitle">The title of the feed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private Task CleanupEpisodesAsync(PodcastFeed feed, string feedTitle)
    {
        if (!this.FeedConfig.Count.HasValue)
            return Task.CompletedTask;

        var basePattern = Utils.Utils.SanitizeFolderName(feedTitle) + " (";
        var allYearFolders = Directory.GetDirectories(this.OutputPath)
            .Where(d => Path.GetFileName(d).StartsWith(basePattern))
            .ToList();

        var allFiles = allYearFolders
            .SelectMany(folder => Directory.GetFiles(folder, "*.*")
                .Select(f => new
                {
                    FilePath = f,
                    Date = DateTime.TryParseExact(
                        Path.GetFileName(f).Substring(0, 10),
                        "yyyy-MM-dd",
                        null,
                        System.Globalization.DateTimeStyles.None,
                        out var dt) ? dt : DateTime.MinValue
                }))
            .OrderByDescending(x => x.Date)
            .ToList();

        var toDelete = allFiles.Skip(this.FeedConfig.Count.Value);

        foreach (var file in toDelete)
        {
            try
            {
                Logger.Info("Deleting old episode: {0}", file.FilePath);
                File.Delete(file.FilePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Delete episode \"{0}\" failed.", file.FilePath);
            }
        }

        return Task.CompletedTask;
    }

    #endregion
}
