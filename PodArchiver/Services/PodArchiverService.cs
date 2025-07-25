using System.Text.Json;
using NLog;
using PodArchiver.Models;

namespace PodArchiver.Services;

/// <summary>
/// Service for downloading and managing podcast episodes based on a configuration file.
/// Handles scheduling, configuration loading, and orchestrates the download and cleanup process.
/// </summary>
public class PodArchiverService
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PodArchiverService"/> class.
    /// </summary>
    /// <param name="configFile">The path to the configuration file.</param>
    public PodArchiverService(string configFile)
    {
        this.ConfigFile = configFile;
    }

    #endregion


    #region Properties

    /// <summary>
    /// Gets the logger instance for logging operations.
    /// </summary>
    private static Logger Logger { get; } = LogManager.GetLogger(nameof(PodArchiverService));

    /// <summary>
    /// Gets the path to the configuration file.
    /// </summary>
    private string ConfigFile { get; }

    /// <summary>
    /// Gets or sets the loaded application configuration.
    /// </summary>
    private AppConfig? Config { get; set; }

    #endregion


    #region Public Methods

    /// <summary>
    /// Runs the podcast download service, including scheduling and repeated execution.
    /// </summary>
    /// <param name="token">A cancellation token to observe while waiting for tasks to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RunAsync(CancellationToken token)
    {
        await this.LoadConfigAsync(token);

        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        Logger.Info("PodArchiver started (Version: {0})", version);
        Logger.Info("Output path: {0}", this.Config!.OutputPath);

        var downloadTimes = this.Config.DownloadTimes is { Count: > 0 }
            ? this.Config.DownloadTimes
            : new List<TimeSpan> { new(2, 0, 0) }; // Default 02:00

        while (!token.IsCancellationRequested)
        {
            await this.DownloadAllAsync(token);

            var now = DateTime.Now;
            var nextDownloadTimes = downloadTimes
                .Select(ts => now.Date + ts)
                .Concat(downloadTimes.Select(ts => now.Date.AddDays(1) + ts))
                .Where(dt => dt > now)
                .OrderBy(dt => dt)
                .ToList();

            var nextDownloadTime = nextDownloadTimes.First();
            var delay = nextDownloadTime - now;

            Logger.Info("Waiting until {0:yyyy-MM-dd HH:mm} ({1:F0} minutes)...", nextDownloadTime, delay.TotalMinutes);
            await Task.Delay(delay, token);
        }
    }

    #endregion


    #region Private Methods

    /// <summary>
    /// Loads the application configuration from the specified configuration file.
    /// </summary>
    /// <param name="token">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LoadConfigAsync(CancellationToken token)
    {
        var json = await File.ReadAllTextAsync(this.ConfigFile, token);
        this.Config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
    }

    /// <summary>
    /// Downloads and processes all podcast feeds as specified in the configuration.
    /// </summary>
    /// <param name="token">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task DownloadAllAsync(CancellationToken token)
    {
        Logger.Info("Archiving {0} podcast feeds", this.Config!.Feeds.Count);

        foreach (var feedConfig in this.Config.Feeds)
        {
            token.ThrowIfCancellationRequested();

            var downloader = new PodArchiver(feedConfig, this.Config.OutputPath, token);
            await downloader.RunAsync();
        }
    }

    #endregion
}
