using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace PodArchiver.Utils;

public static class Utils
{
    #region Public Methods

    public static string SanitizeFolderName(string name)
    {
        char[] extraInvalid = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
        var invalidChars = Path.GetInvalidPathChars().Union(extraInvalid).ToArray();
        return string.Concat(name.Where(c => !invalidChars.Contains(c))).Trim();
    }

    public static string SanitizeFileName(string name)
    {
        char[] extraInvalid = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
        var invalidChars = Path.GetInvalidFileNameChars().Union(extraInvalid).ToArray();
        return string.Concat(name.Where(c => !invalidChars.Contains(c))).Trim();
    }

    public static byte[] ResizeToJpeg(byte[] imageData, int maxSize = 800)
    {
        using var input = new MemoryStream(imageData);
        using var image = Image.Load(input);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(maxSize, maxSize),
            Mode = ResizeMode.Max
        }));

        using var output = new MemoryStream();
        image.SaveAsJpeg(output, new JpegEncoder { Quality = 90 });
        return output.ToArray();
    }

    #endregion
}