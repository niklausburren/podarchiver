using System.Xml.Linq;
using PodArchiver.Models;

namespace PodArchiver.Services;

public class RssParser
{
    #region Fields

    private readonly XNamespace itunes = "http://www.itunes.com/dtds/podcast-1.0.dtd";

    #endregion


    #region Constructors

    public RssParser(string url, HttpClient http)
    {
        this.Url = url;
        this.Http = http;
    }

    #endregion


    #region Properties

    public string Url { get; }

    public HttpClient Http { get; }

    #endregion


    #region Public Methods

    public async Task<PodcastFeed> ParseAsync()
    {
        await using var stream = await this.Http.GetStreamAsync(this.Url);
        var feed = XDocument.Load(stream);

        var channelElement = feed.Root?.Element("channel")!;
        var title = channelElement.Element("title")?.Value ?? "Unknown";
        var coverUrl = channelElement.Element("image")?.Element("url")?.Value;

        byte[]? coverBytes = null;

        if (!string.IsNullOrWhiteSpace(coverUrl))
        {
            try
            {
                coverBytes = await this.Http.GetByteArrayAsync(coverUrl);
                coverBytes = Utils.Utils.ResizeToJpeg(coverBytes);
            }
            catch
            {
                // ignored
            }
        }

        var episodes = channelElement.Elements("item")
            .Select(item =>
            {
                var categories = channelElement.Elements(itunes + "category")
                    .Select(cat => cat.Attribute("text")?.Value ?? cat.Value)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Prepend("Podcast")
                    .ToList();

                return new PodcastEpisode(
                    url: item.Element("enclosure")?.Attribute("url")?.Value ?? string.Empty,
                    title: item.Element("title")?.Value ?? "Unknown",
                    pubDate: DateTime.TryParse(item.Element("pubDate")?.Value, out var pubDateParsed) ? pubDateParsed : DateTime.UtcNow,
                    authors: item.Element(itunes + "author")?.Value.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToList() ?? [],
                    categories: categories,
                    description: item.Element("description")?.Value ?? string.Empty
                );
            })
            .Where(e => !string.IsNullOrWhiteSpace(e.Url))
            .ToList();

        return new PodcastFeed(title, episodes, coverBytes);
    }

    #endregion
}