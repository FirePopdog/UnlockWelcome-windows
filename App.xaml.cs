using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using UnlockWelcome.Services;
using UnlockWelcome.Views;
using Application = System.Windows.Application;

namespace UnlockWelcome;

public partial class App : Application
{
    private Mutex? _mutex;
    private SessionUnlockMonitor? _unlockMonitor;
    private NotifyIcon? _trayIcon;
    private MainWindow? _settingsWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        _mutex = new Mutex(true, "UnlockWelcome.SingleInstance", out var createdNew);
        if (!createdNew)
        {
            Shutdown();
            return;
        }

        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        _unlockMonitor = new SessionUnlockMonitor();
        _unlockMonitor.Unlocked += OnSessionUnlocked;
        _unlockMonitor.Start();
        CreateTrayIcon();

        if (e.Args.Contains("--preview", StringComparer.OrdinalIgnoreCase))
            ShowWelcome();
        if (e.Args.Contains("--settings", StringComparer.OrdinalIgnoreCase))
            ShowSettings();
    }

    private void CreateTrayIcon()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("播放歡迎動畫", null, (_, _) => ShowWelcome());
        menu.Items.Add("設定", null, (_, _) => ShowSettings());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("結束 UnlockWelcome", null, (_, _) => Shutdown());

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "UnlockWelcome",
            ContextMenuStrip = menu,
            Visible = true
        };
        _trayIcon.DoubleClick += (_, _) => ShowSettings();
    }

    private void OnSessionUnlocked(object? sender, EventArgs e) => Dispatcher.BeginInvoke(ShowWelcome);

    private void ShowWelcome() => new WelcomeWindow().Show();

    private void ShowSettings()
    {
        if (_settingsWindow is null)
        {
            _settingsWindow = new MainWindow();
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        }

        if (!_settingsWindow.IsVisible)
            _settingsWindow.Show();
        _settingsWindow.Activate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _unlockMonitor?.Dispose();
        _trayIcon?.Dispose();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}