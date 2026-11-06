using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using UnlockWelcome.Models;
using UnlockWelcome.Services;
using UnlockWelcome.Views;
using Application = System.Windows.Application;
using Forms = System.Windows.Forms;

namespace UnlockWelcome;

public partial class App : Application
{
    private const string MutexName = "UnlockWelcome.SingleInstance";
    private const string SettingsSignalName = "Local\\UnlockWelcome.ShowSettings";

    private Mutex? _mutex;
    private EventWaitHandle? _settingsSignal;
    private CancellationTokenSource? _signalCancellation;
    private SessionUnlockMonitor? _unlockMonitor;
    private NotifyIcon? _trayIcon;
    private MainWindow? _settingsWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        var mutex = new Mutex(true, MutexName, out var createdNew);
        if (!createdNew)
        {
            mutex.Dispose();
            try
            {
                using var signal = EventWaitHandle.OpenExisting(SettingsSignalName);
                signal.Set();
            }
            catch
            {
            }

            Shutdown();
            return;
        }

        _mutex = mutex;
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        StartSettingsSignalListener();

        _unlockMonitor = new SessionUnlockMonitor();
        _unlockMonitor.Unlocked += OnSessionUnlocked;
        _unlockMonitor.Start();
        CreateTrayIcon();

        var preview = e.Args.Contains("--preview", StringComparer.OrdinalIgnoreCase);
        var background = e.Args.Contains("--background", StringComparer.OrdinalIgnoreCase);
        var settings = e.Args.Contains("--settings", StringComparer.OrdinalIgnoreCase);

        if (preview)
            ShowWelcome();
        if (settings || (!background && !preview))
            ShowSettings();
    }

    private void StartSettingsSignalListener()
    {
        _settingsSignal = new EventWaitHandle(false, EventResetMode.AutoReset, SettingsSignalName);
        _signalCancellation = new CancellationTokenSource();
        _ = Task.Run(() =>
        {
            while (!_signalCancellation.IsCancellationRequested)
            {
                _settingsSignal.WaitOne();
                if (!_signalCancellation.IsCancellationRequested)
                    Dispatcher.BeginInvoke(ShowSettings);
            }
        });
    }

    private void CreateTrayIcon()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("開啟動畫工作室", null, (_, _) => ShowSettings());
        menu.Items.Add("立即預覽動畫", null, (_, _) => ShowWelcome());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("結束 UnlockWelcome", null, (_, _) => Shutdown());

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "UnlockWelcome Studio",
            ContextMenuStrip = menu,
            Visible = true
        };
        _trayIcon.DoubleClick += (_, _) => ShowSettings();
    }

    private void OnSessionUnlocked(object? sender, EventArgs e) =>
        Dispatcher.BeginInvoke(ShowWelcome);

    private void ShowWelcome()
    {
        if (AppSettings.Current.DisplayTarget == "All")
        {
            foreach (var screen in Forms.Screen.AllScreens)
                new WelcomeWindow(screen).Show();
        }
        else
        {
            new WelcomeWindow(Forms.Screen.PrimaryScreen).Show();
        }
    }

    private void ShowSettings()
    {
        if (_settingsWindow is null)
        {
            _settingsWindow = new MainWindow();
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        }

        if (!_settingsWindow.IsVisible)
            _settingsWindow.Show();
        if (_settingsWindow.WindowState == WindowState.Minimized)
            _settingsWindow.WindowState = WindowState.Normal;
        _settingsWindow.Activate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _signalCancellation?.Cancel();
        _settingsSignal?.Set();
        _settingsSignal?.Dispose();
        _signalCancellation?.Dispose();
        _unlockMonitor?.Dispose();
        _trayIcon?.Dispose();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}