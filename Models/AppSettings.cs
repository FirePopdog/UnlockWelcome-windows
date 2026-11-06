using System.IO;
using System.Text.Json;

namespace UnlockWelcome.Models;

public sealed class AppSettings
{
    private static readonly string DirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UnlockWelcome");
    private static readonly string FilePath = Path.Combine(DirectoryPath, "settings.json");

    public static AppSettings Current { get; private set; } = Load();

    public string ThemeName { get; set; } = "Cinematic";
    public string Greeting { get; set; } = "Welcome back.";
    public string Subtitle { get; set; } = "很高興再次見到你。";
    public string AccentColor { get; set; } = "#91A9FF";
    public string BackgroundColor { get; set; } = "#070910";
    public string TitleColor { get; set; } = "#F5F7FF";
    public string BackgroundImagePath { get; set; } = "";
    public string BackgroundVideoPath { get; set; } = "";
    public string AnimationStyle { get; set; } = "Fade";
    public string ParticleSpeed { get; set; } = "Normal";
    public string DisplayTarget { get; set; } = "Primary";
    public string TextAlignment { get; set; } = "Center";
    public string FontFamilyName { get; set; } = "Segoe UI";
    public int DurationSeconds { get; set; } = 5;
    public int FontSize { get; set; } = 58;
    public int ParticleCount { get; set; } = 34;
    public bool ShowDate { get; set; } = true;
    public bool ShowParticles { get; set; } = true;
    public bool EnableGlow { get; set; } = true;

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

    public static void Replace(AppSettings settings)
    {
        Current = settings;
        Save();
    }
}