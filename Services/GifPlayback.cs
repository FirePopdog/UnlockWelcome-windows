using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;

namespace UnlockWelcome.Services;

public sealed class GifPlayback : IDisposable
{
    private readonly Image _target;
    private readonly BitmapFrame[] _frames;
    private readonly int[] _delays;
    private readonly DispatcherTimer _timer;
    private int _frameIndex;
    private bool _disposed;

    public GifPlayback(Image target, string path)
    {
        _target = target;
        var decoder = new GifBitmapDecoder(
            new Uri(path, UriKind.Absolute),
            BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnLoad);

        _frames = decoder.Frames.ToArray();
        if (_frames.Length == 0)
            throw new InvalidDataException("GIF 檔案沒有可播放的影格。");

        _delays = _frames.Select(ReadDelayMilliseconds).ToArray();
        _target.Source = _frames[0];

        _timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(_delays[0])
        };
        _timer.Tick += OnTick;

        if (_frames.Length > 1)
            _timer.Start();
    }

    private static int ReadDelayMilliseconds(BitmapFrame frame)
    {
        try
        {
            if (frame.Metadata is BitmapMetadata metadata)
            {
                var delay = metadata.GetQuery("/grctlext/Delay");
                if (delay is ushort units)
                    return Math.Clamp(units * 10, 20, 2000);
                if (delay is byte byteUnits)
                    return Math.Clamp(byteUnits * 10, 20, 2000);
            }
        }
        catch
        {
        }

        return 100;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_disposed || _frames.Length < 2)
            return;

        _frameIndex = (_frameIndex + 1) % _frames.Length;
        _target.Source = _frames[_frameIndex];
        _timer.Interval = TimeSpan.FromMilliseconds(_delays[_frameIndex]);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _timer.Stop();
        _timer.Tick -= OnTick;
        _target.Source = null;
    }
}