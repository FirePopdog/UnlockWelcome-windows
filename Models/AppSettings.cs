using System.IO;
using System.Text.Json;

namespace UnlockWelcome.Models;

public sealed class AppSettings
{
    private static readonly string DirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UnlockWelcome");
    private static readonly string FilePath = Path.Combine(DirectoryPath, "settings.json");

    public static AppSettings Current { get; private set; } = Load();

    public string Greeting { get; set; } = "Welcome back.";
    public string Subtitle { get; set; } = "很高興再次見到你。";
    public int DurationSeconds { get; set; } = 5;

    private static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new AppSettings();
        }
        catch
        {
        }
        return new AppSettings();
    }

    public static void Save()
    {
        Directory.CreateDirectory(DirectoryPath);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true }));
    }
}