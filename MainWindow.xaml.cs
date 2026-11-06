using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using UnlockWelcome.Models;
using UnlockWelcome.Services;
using UnlockWelcome.Views;
using Forms = System.Windows.Forms;

namespace UnlockWelcome;

public partial class MainWindow : Window
{
    private bool _loading;

    public MainWindow()
    {
        InitializeComponent();
        FontSizeSlider.ValueChanged += FontSizeSlider_ValueChanged;
        ParticleCountSlider.ValueChanged += ParticleCountSlider_ValueChanged;
        _loading = true;
        LoadControls();
        _loading = false;
    }

    private void LoadControls()
    {
        var settings = AppSettings.Current;
        SetTheme(settings.ThemeName);
        GreetingTextBox.Text = settings.Greeting;
        SubtitleTextBox.Text = settings.Subtitle;
        AccentColorTextBox.Text = settings.AccentColor;
        BackgroundColorTextBox.Text = settings.BackgroundColor;
        TitleColorTextBox.Text = settings.TitleColor;
        BackgroundImagePathTextBox.Text = string.IsNullOrWhiteSpace(settings.BackgroundImagePath) ? "尚未選擇背景圖片" : settings.BackgroundImagePath;
        BackgroundVideoPathTextBox.Text = string.IsNullOrWhiteSpace(settings.BackgroundVideoPath) ? "尚未選擇影片" : settings.BackgroundVideoPath;
        ShowDateCheckBox.IsChecked = settings.ShowDate;
        ParticlesCheckBox.IsChecked = settings.ShowParticles;
        GlowCheckBox.IsChecked = settings.EnableGlow;
        FontSizeSlider.Value = Math.Clamp(settings.FontSize, 28, 100);
        ParticleCountSlider.Value = Math.Clamp(settings.ParticleCount, 0, 100);
        SetTag(AnimationStyleComboBox, settings.AnimationStyle);
        SetTag(DurationComboBox, settings.DurationSeconds.ToString());
        SetTag(ParticleSpeedComboBox, settings.ParticleSpeed);
        SetTag(DisplayTargetComboBox, settings.DisplayTarget);
        SetTag(TextAlignmentComboBox, settings.TextAlignment);
        SetTag(FontFamilyComboBox, settings.FontFamilyName);
        StartupCheckBox.IsChecked = StartupManager.IsEnabled;
        UpdateSliderLabels();
    }

    private static void SetTag(System.Windows.Controls.ComboBox combo, string tag)
    {
        combo.SelectedItem = combo.Items.Cast<ComboBoxItem>()
            .FirstOrDefault(item => string.Equals(item.Tag?.ToString(), tag, StringComparison.OrdinalIgnoreCase));
        if (combo.SelectedIndex < 0 && combo.Items.Count > 0)
            combo.SelectedIndex = 0;
    }

    private void SetTheme(string theme)
    {
        var index = ThemeComboBox.Items.Cast<ComboBoxItem>()
            .ToList().FindIndex(item => string.Equals(item.Content?.ToString(), theme, StringComparison.OrdinalIgnoreCase));
        ThemeComboBox.SelectedIndex = index >= 0 ? index : 6;
        ActiveThemeLabel.Text = theme;
    }

    private void ThemeCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is Border card && card.Tag is string theme)
        {
            var item = ThemeComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content?.ToString() == theme);
            if (item is not null) ThemeComboBox.SelectedItem = item;
        }
    }

    private void Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || !IsLoaded || ThemeComboBox.SelectedItem is not ComboBoxItem selected)
            return;

        var theme = selected.Content?.ToString() ?? "Custom";
        ActiveThemeLabel.Text = theme;
        switch (theme)
        {
            case "Cinematic":
                SetColors("#91A9FF", "#070910", "#F5F7FF");
                SetTag(AnimationStyleComboBox, "Fade");
                ParticlesCheckBox.IsChecked = true;
                ParticleCountSlider.Value = 34;
                SetTag(ParticleSpeedComboBox, "Normal");
                GlowCheckBox.IsChecked = true;
                break;
            case "Aurora":
                SetColors("#72E4CF", "#071A20", "#E5FFFB");
                SetTag(AnimationStyleComboBox, "Slide");
                ParticlesCheckBox.IsChecked = true;
                ParticleCountSlider.Value = 58;
                SetTag(ParticleSpeedComboBox, "Slow");
                GlowCheckBox.IsChecked = true;
                break;
            case "Minimal":
                SetColors("#50627E", "#E9EDF4", "#253248");
                SetTag(AnimationStyleComboBox, "Fade");
                ParticlesCheckBox.IsChecked = false;
                ParticleCountSlider.Value = 0;
                GlowCheckBox.IsChecked = false;
                break;
            case "Neon":
                SetColors("#D16CFF", "#100719", "#FFF1FF");
                SetTag(AnimationStyleComboBox, "Zoom");
                ParticlesCheckBox.IsChecked = true;
                ParticleCountSlider.Value = 78;
                SetTag(ParticleSpeedComboBox, "Fast");
                GlowCheckBox.IsChecked = true;
                break;
            case "Midnight":
                SetColors("#D9B979", "#100F10", "#F6EBD7");
                SetTag(AnimationStyleComboBox, "Fade");
                ParticlesCheckBox.IsChecked = true;
                ParticleCountSlider.Value = 18;
                SetTag(ParticleSpeedComboBox, "Slow");
                GlowCheckBox.IsChecked = true;
                break;
            case "Fluent":
                SetColors("#8DBBFF", "#111C2C", "#F0F6FF");
                SetTag(AnimationStyleComboBox, "Zoom");
                ParticlesCheckBox.IsChecked = true;
                ParticleCountSlider.Value = 24;
                SetTag(ParticleSpeedComboBox, "Normal");
                GlowCheckBox.IsChecked = false;
                break;
        }

        UpdateSliderLabels();
    }

    private void SetColors(string accent, string background, string title)
    {
        AccentColorTextBox.Text = accent;
        BackgroundColorTextBox.Text = background;
        TitleColorTextBox.Text = title;
    }

    private void UpdateSliderLabels()
    {
        FontSizeValue.Text = $"{(int)FontSizeSlider.Value} px";
        ParticleCountValue.Text = $"{(int)ParticleCountSlider.Value} 個";
    }

    private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdateSliderLabels();
    private void ParticleCountSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdateSliderLabels();

    private void BrowseBackground_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "選擇歡迎動畫背景圖片",
            Filter = "圖片檔案|*.png;*.jpg;*.jpeg;*.bmp|所有檔案|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            BackgroundImagePathTextBox.Text = dialog.FileName;
            BackgroundVideoPathTextBox.Text = "尚未選擇影片";
        }
    }

    private void BrowseVideoBackground_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "選擇影片背景",
            Filter = "影片檔案|*.mp4;*.wmv;*.avi;*.mpeg;*.mpg|所有檔案|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            BackgroundVideoPathTextBox.Text = dialog.FileName;
            BackgroundImagePathTextBox.Text = "尚未選擇背景圖片";
        }
    }

    private void ClearBackground_Click(object sender, RoutedEventArgs e)
    {
        BackgroundImagePathTextBox.Text = "尚未選擇背景圖片";
        BackgroundVideoPathTextBox.Text = "尚未選擇影片";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveSettings();
            System.Windows.MessageBox.Show("主題與設定已儲存。", "UnlockWelcome Studio",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "設定無法儲存", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void SaveSettings()
    {
        var accent = AccentColorTextBox.Text.Trim();
        var background = BackgroundColorTextBox.Text.Trim();
        var titleColor = TitleColorTextBox.Text.Trim();
        if (!IsValidColor(accent))
            throw new InvalidOperationException("強調色格式不正確，請輸入例如 #91A9FF 的 HEX 顏色。");
        if (!IsValidColor(background))
            throw new InvalidOperationException("背景色格式不正確，請輸入例如 #070910 的 HEX 顏色。");
        if (!IsValidColor(titleColor))
            throw new InvalidOperationException("標題文字色格式不正確，請輸入例如 #F5F7FF 的 HEX 顏色。");

        var settings = AppSettings.Current;
        settings.ThemeName = (ThemeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Custom";
        settings.Greeting = GreetingTextBox.Text.Trim();
        settings.Subtitle = SubtitleTextBox.Text.Trim();
        settings.AccentColor = accent;
        settings.BackgroundColor = background;
        settings.TitleColor = titleColor;
        settings.FontFamilyName = SelectedTag(FontFamilyComboBox, "Segoe UI");
        settings.BackgroundImagePath = File.Exists(BackgroundImagePathTextBox.Text) ? BackgroundImagePathTextBox.Text : "";
        settings.BackgroundVideoPath = File.Exists(BackgroundVideoPathTextBox.Text) ? BackgroundVideoPathTextBox.Text : "";
        settings.ShowDate = ShowDateCheckBox.IsChecked == true;
        settings.ShowParticles = ParticlesCheckBox.IsChecked == true;
        settings.EnableGlow = GlowCheckBox.IsChecked == true;
        settings.FontSize = (int)FontSizeSlider.Value;
        settings.ParticleCount = (int)ParticleCountSlider.Value;
        settings.AnimationStyle = SelectedTag(AnimationStyleComboBox, "Fade");
        settings.ParticleSpeed = SelectedTag(ParticleSpeedComboBox, "Normal");
        settings.DisplayTarget = SelectedTag(DisplayTargetComboBox, "Primary");
        settings.TextAlignment = SelectedTag(TextAlignmentComboBox, "Center");
        settings.DurationSeconds = int.Parse(SelectedTag(DurationComboBox, "5"));
        AppSettings.Save();
        StartupManager.SetEnabled(StartupCheckBox.IsChecked == true);
        ActiveThemeLabel.Text = settings.ThemeName;
    }

    private static bool IsValidColor(string value)
    {
        try
        {
            return System.Windows.Media.ColorConverter.ConvertFromString(value) is System.Windows.Media.Color;
        }
        catch
        {
            return false;
        }
    }

    private static string SelectedTag(System.Windows.Controls.ComboBox combo, string fallback) =>
        (combo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? fallback;

    private void Preview_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveSettings();
            if (AppSettings.Current.DisplayTarget == "All")
            {
                foreach (var screen in Forms.Screen.AllScreens)
                    new WelcomeWindow(screen, true).Show();
            }
            else
            {
                new WelcomeWindow(Forms.Screen.PrimaryScreen, true).Show();
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "無法預覽", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Export_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveSettings();
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "匯出 UnlockWelcome 主題",
                Filter = "UnlockWelcome Theme|*.uwtheme.json|JSON 檔案|*.json",
                FileName = $"{AppSettings.Current.ThemeName}.uwtheme.json"
            };
            if (dialog.ShowDialog() != true)
                return;

            var json = JsonSerializer.Serialize(AppSettings.Current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dialog.FileName, json);
            System.Windows.MessageBox.Show("主題已匯出，可以分享給其他 UnlockWelcome 使用者。背景圖片或影片仍需另外分享原始檔案。", "匯出完成",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "匯出失敗", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Import_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "匯入 UnlockWelcome 主題",
            Filter = "UnlockWelcome Theme|*.uwtheme.json;*.json|JSON 檔案|*.json",
            CheckFileExists = true
        };
        if (dialog.ShowDialog() != true)
            return;

        try
        {
            var imported = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(dialog.FileName))
                ?? throw new InvalidDataException("主題檔案內容是空的或格式不正確。");

            if (string.IsNullOrWhiteSpace(imported.Greeting) || !IsValidColor(imported.AccentColor) ||
                !IsValidColor(imported.BackgroundColor) || !IsValidColor(imported.TitleColor))
                throw new InvalidDataException("主題檔案缺少必要設定或顏色格式不正確。");

            AppSettings.Replace(imported);
            _loading = true;
            LoadControls();
            _loading = false;
            System.Windows.MessageBox.Show("主題已匯入並套用。", "匯入完成",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _loading = false;
            System.Windows.MessageBox.Show(ex.Message, "匯入失敗", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}