using PodArchiver.Models;
using TagLib;

namespace PodArchiver.Tagging;

public class TagWriter
{
    #region Fields

    private readonly string _filePath;

    #endregion


    #region Constructors

    public TagWriter(string filePath)
    {
        _filePath = filePath;
    }

    #endregion


    #region Public Methods

    public void ClearAllTags()
    {
        var file = TagLib.File.Create(_filePath);
        file.RemoveTags(TagTypes.Id3v1 | TagTypes.Id3v2);
        file.Save();
    }

    public void WriteTags(PodcastEpisode episode, string album, byte[]? coverImage, IEnumerable<string> albumArtists)
    {
        var file = TagLib.File.Create(_filePath);

        file.Tag.Title = $"{episode.PubDate:dd.MM.} {episode.Title}";
        file.Tag.Track = episode.Number;
        file.Tag.Album = album;
        file.Tag.Performers = episode.Authors.ToArray();
        file.Tag.AlbumArtists = albumArtists.ToArray();
        file.Tag.Genres = episode.Categories.ToArray();
        file.Tag.Year = (uint)(episode.PubDate.Year is >= 1900 and <= 2100 ? episode.PubDate.Year : DateTime.UtcNow.Year);
        file.Tag.Comment = episode.Description;

        if (coverImage is not null)
        {
            var mimeType = coverImage.Length >= 4 && coverImage[0] == 0x89 && coverImage[1] == 0x50
                ? "image/png"
                : "image/jpeg";

            file.Tag.Pictures =
            [
                new Picture
                {
                    Type = PictureType.FrontCover,
                    Description = "Cover",
                    MimeType = mimeType,
                    Data = coverImage
                }
            ];
        }

        file.Save();
    }

    #endregion
}