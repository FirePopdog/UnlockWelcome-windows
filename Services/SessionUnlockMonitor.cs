using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace UnlockWelcome.Services;

public sealed class SessionUnlockMonitor : IDisposable
{
    private const int WmWtsSessionChange = 0x02B1;
    private const int WtsSessionUnlock = 0x8;
    private const int NotifyForThisSession = 0;
    private readonly HwndSource _source;
    private bool _registered;

    public event EventHandler? Unlocked;

    public SessionUnlockMonitor()
    {
        var parameters = new HwndSourceParameters("UnlockWelcome.SessionMonitor")
        {
            Width = 0,
            Height = 0,
            PositionX = -32000,
            PositionY = -32000,
            WindowStyle = unchecked((int)0x80000000)
        };
        _source = new HwndSource(parameters);
        _source.AddHook(WindowProc);
    }

    public void Start()
    {
        _registered = WTSRegisterSessionNotification(_source.Handle, NotifyForThisSession);
        if (!_registered)
            throw new InvalidOperationException("Unable to register for Windows session notifications. Error: " + Marshal.GetLastWin32Error());
    }

    private IntPtr WindowProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == WmWtsSessionChange && wParam.ToInt32() == WtsSessionUnlock)
        {
            handled = true;
            Unlocked?.Invoke(this, EventArgs.Empty);
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_registered)
            WTSUnRegisterSessionNotification(_source.Handle);
        _source.RemoveHook(WindowProc);
        _source.Dispose();
    }

    [DllImport("wtsapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool WTSRegisterSessionNotification(IntPtr hWnd, int dwFlags);

    [DllImport("wtsapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool WTSUnRegisterSessionNotification(IntPtr hWnd);
}