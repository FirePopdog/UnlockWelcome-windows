using System.Windows;
using System.Windows.Controls;
using UnlockWelcome.Models;
using UnlockWelcome.Services;
using UnlockWelcome.Views;

namespace UnlockWelcome;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        StartupCheckBox.IsChecked = StartupManager.IsEnabled;
        GreetingTextBox.Text = AppSettings.Current.Greeting;
        SubtitleTextBox.Text = AppSettings.Current.Subtitle;
        DurationComboBox.SelectedIndex = AppSettings.Current.DurationSeconds switch { 3 => 0, 5 => 1, 8 => 2, 0 => 3, _ => 1 };
    }

    private void Preview_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings(false);
        new WelcomeWindow(true).Show();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings(true);
        System.Windows.MessageBox.Show("設定已儲存。", "UnlockWelcome", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SaveSettings(bool saveStartup)
    {
        var selected = (ComboBoxItem)DurationComboBox.SelectedItem;
        AppSettings.Current.Greeting = GreetingTextBox.Text.Trim();
        AppSettings.Current.Subtitle = SubtitleTextBox.Text.Trim();
        AppSettings.Current.DurationSeconds = int.Parse(selected.Tag.ToString()!);
        AppSettings.Save();

        if (saveStartup)
            StartupManager.SetEnabled(StartupCheckBox.IsChecked == true);
    }
}