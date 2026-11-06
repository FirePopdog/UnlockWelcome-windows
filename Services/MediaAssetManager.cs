using System.IO;

namespace UnlockWelcome.Services;

public static class MediaAssetManager
{
    private static readonly string MediaDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "UnlockWelcome", "Media");

    public static string CopyIntoLibrary(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            return "";

        Directory.CreateDirectory(MediaDirectory);
        var fullSource = Path.GetFullPath(sourcePath);
        var fullDirectory = Path.GetFullPath(MediaDirectory) + Path.DirectorySeparatorChar;

        if (fullSource.StartsWith(fullDirectory, StringComparison.OrdinalIgnoreCase))
            return fullSource;

        var extension = Path.GetExtension(fullSource).ToLowerInvariant();
        var allowed = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".mp4", ".wmv", ".avi", ".mpg", ".mpeg" };
        if (!allowed.Contains(extension, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("不支援這種媒體格式。");

        var destination = Path.Combine(MediaDirectory, $"{Guid.NewGuid():N}{extension}");
        File.Copy(fullSource, destination, false);
        return destination;
    }

    public static string GetThemeImportDirectory() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "UnlockWelcome", "ImportedThemes", Guid.NewGuid().ToString("N"));
}