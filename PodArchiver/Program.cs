using NLog;
using PodArchiver.Services;

namespace PodArchiver;

public class Program
{
    #region Public Methods

    public static async Task<int> Main(string[] args)
    {
        var logger = LogManager.GetCurrentClassLogger();
        const string configFile = "config.json";

        var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            logger.Info("Cancellation requested (CTRL+C) ...");
            cts.Cancel();
            e.Cancel = true;
        };

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            logger.Info("Process exit signal received ...");
            cts.Cancel();
        };

        try
        {
            var service = new PodArchiverService(configFile);
            await service.RunAsync(cts.Token);
            return 0; // success
        }
        catch (OperationCanceledException)
        {
            return 0; // treat cancellation as success
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unhandled exception occurred.");
            return 1; // error
        }
        finally
        {
            logger.Info("Podcast downloader stopped");
        }
    }

    #endregion
}