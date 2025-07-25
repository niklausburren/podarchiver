namespace PodArchiver.Models;

public class PodcastEpisode
{
    #region Constructors

    public PodcastEpisode(
        string url,
        string title,
        DateTime pubDate,
        IReadOnlyList<string> authors,
        IReadOnlyList<string> categories,
        string description)
    {
        this.Title = title;
        this.Url = url;
        this.PubDate = pubDate;
        this.Authors = authors;
        this.Categories = categories;
        this.Description = description;
    }

    #endregion


    #region Properties

    public string Title { get; init; }

    public string Url { get; init; }

    public DateTime PubDate { get; init; }

    public uint Number
    {
        get
        {
            var daysInYear = DateTime.IsLeapYear(this.PubDate.Year) ? 366 : 365;
            return (uint)(daysInYear + 1 - this.PubDate.DayOfYear);
        }
    }

    public IReadOnlyList<string> Authors { get; init; }

    public IReadOnlyList<string> Categories { get; init; }

    public string Description { get; init; }

    public string Extension => Path.GetExtension(new Uri(this.Url).LocalPath);

    #endregion
}
