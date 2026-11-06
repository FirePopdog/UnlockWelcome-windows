using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using UnlockWelcome.Models;
using Forms = System.Windows.Forms;

namespace UnlockWelcome.Views;

public partial class WelcomeWindow : Window
{
    private readonly DispatcherTimer _closeTimer = new();
    private readonly Random _random = new();

    public WelcomeWindow(Forms.Screen? screen = null, bool preview = false)
    {
        InitializeComponent();
        var settings = AppSettings.Current;
        ApplySettings(settings);
        if (screen is not null)
        {
            Left = screen.Bounds.Left;
            Top = screen.Bounds.Top;
            Width = screen.Bounds.Width;
            Height = screen.Bounds.Height;
        }
        else
        {
            WindowState = WindowState.Maximized;
        }

        Loaded += (_, _) => PlayAnimation(settings);
        if (settings.DurationSeconds > 0)
        {
            _closeTimer.Interval = TimeSpan.FromSeconds(settings.DurationSeconds);
            _closeTimer.Tick += (_, _) => Close();
            _closeTimer.Start();
        }
    }

    private void ApplySettings(AppSettings settings)
    {
        Greeting.Text = settings.Greeting;
        Subtitle.Text = settings.Subtitle;
        Greeting.FontFamily = new System.Windows.Media.FontFamily(string.IsNullOrWhiteSpace(settings.FontFamilyName) ? "Segoe UI" : settings.FontFamilyName);
        Subtitle.FontFamily = Greeting.FontFamily;
        DateText.FontFamily = Greeting.FontFamily;
        DateText.Text = DateTime.Now.ToString("dddd, MMMM d  •  HH:mm");
        Subtitle.Visibility = string.IsNullOrWhiteSpace(settings.Subtitle) ? Visibility.Collapsed : Visibility.Visible;
        DateText.Visibility = settings.ShowDate ? Visibility.Visible : Visibility.Collapsed;
        Greeting.FontSize = Math.Clamp(settings.FontSize, 28, 100);
        Greeting.Foreground = BrushFromHex(settings.TitleColor, "#F5F7FF");
        Subtitle.Foreground = BrushFromHex(settings.AccentColor, "#AAB5CC");
        Eyebrow.Foreground = BrushFromHex(settings.AccentColor, "#91A9E8");
        AccentLine.Background = BrushFromHex(settings.AccentColor, "#91A9FF");
        GlowOrb.Fill = BrushFromHex(settings.AccentColor, "#91A9FF");
        Root.Background = BrushFromHex(settings.BackgroundColor, "#070910");
        Greeting.TextAlignment = ToTextAlignment(settings.TextAlignment);
        Subtitle.TextAlignment = ToTextAlignment(settings.TextAlignment);
        DateText.TextAlignment = ToTextAlignment(settings.TextAlignment);

        if (settings.EnableGlow)
        {
            Greeting.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 34,
                ShadowDepth = 0,
                Color = ColorFromHex(settings.AccentColor, Colors.LightBlue),
                Opacity = 0.38
            };
        }
        else
        {
            Greeting.Effect = null;
            GlowOrb.Visibility = Visibility.Collapsed;
        }

        if (!string.IsNullOrWhiteSpace(settings.BackgroundVideoPath) && File.Exists(settings.BackgroundVideoPath))
        {
            try
            {
                BackgroundVideo.Source = new Uri(settings.BackgroundVideoPath, UriKind.Absolute);
                BackgroundVideo.Visibility = Visibility.Visible;
                BackgroundOverlay.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(105, 0, 0, 0));
                return;
            }
            catch
            {
                BackgroundVideo.Visibility = Visibility.Collapsed;
            }
        }

        if (!string.IsNullOrWhiteSpace(settings.BackgroundImagePath) && File.Exists(settings.BackgroundImagePath))
        {
            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(settings.BackgroundImagePath, UriKind.Absolute);
                image.EndInit();
                image.Freeze();
                BackgroundImage.Source = image;
                BackgroundImage.Visibility = Visibility.Visible;
                BackgroundOverlay.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(105, 0, 0, 0));
            }
            catch
            {
                BackgroundImage.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void BackgroundVideo_MediaOpened(object sender, RoutedEventArgs e) => BackgroundVideo.Play();

    private void BackgroundVideo_MediaEnded(object sender, RoutedEventArgs e)
    {
        BackgroundVideo.Position = TimeSpan.Zero;
        BackgroundVideo.Play();
    }

    private static TextAlignment ToTextAlignment(string value) => value switch
    {
        "Left" => TextAlignment.Left,
        "Right" => TextAlignment.Right,
        _ => TextAlignment.Center
    };

    private static System.Windows.Media.Brush BrushFromHex(string value, string fallback)
    {
        try
        {
            return new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(value));
        }
        catch
        {
            return new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(fallback));
        }
    }

    private static System.Windows.Media.Color ColorFromHex(string value, System.Windows.Media.Color fallback)
    {
        try
        {
            return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(value);
        }
        catch
        {
            return fallback;
        }
    }

    private void PlayAnimation(AppSettings settings)
    {
        var style = settings.AnimationStyle;
        Root.Opacity = 0;

        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(950))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Root.BeginAnimation(OpacityProperty, fadeIn);

        if (style.Equals("Zoom", StringComparison.OrdinalIgnoreCase))
        {
            ZoomTransform.ScaleX = 0.90;
            ZoomTransform.ScaleY = 0.90;
            var zoom = new DoubleAnimation(0.90, 1, TimeSpan.FromMilliseconds(1150))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            ZoomTransform.BeginAnimation(ScaleTransform.ScaleXProperty, zoom);
            ZoomTransform.BeginAnimation(ScaleTransform.ScaleYProperty, zoom);
        }
        else if (style.Equals("Slide", StringComparison.OrdinalIgnoreCase))
        {
            SlideTransform.Y = 56;
            var slide = new DoubleAnimation(56, 0, TimeSpan.FromMilliseconds(1100))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            SlideTransform.BeginAnimation(TranslateTransform.YProperty, slide);
        }

        AnimateElement(Eyebrow, 90, 1, 500);
        AnimateElement(Greeting, 220, 1, 850);
        AnimateElement(Subtitle, 520, 1, 750);
        AnimateElement(AccentLine, 700, 1, 650);
        AnimateElement(DateText, 920, 1, 600);

        if (settings.ShowParticles && settings.ParticleCount > 0)
            CreateParticles(settings);
        else
            ParticleCanvas.Children.Clear();
    }

    private static void AnimateElement(UIElement element, double delay, double opacity, double duration)
    {
        var animation = new DoubleAnimation(0, opacity, TimeSpan.FromMilliseconds(duration))
        {
            BeginTime = TimeSpan.FromMilliseconds(delay),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        element.BeginAnimation(OpacityProperty, animation);
    }

    private void CreateParticles(AppSettings settings)
    {
        ParticleCanvas.Children.Clear();
        var width = Math.Max(ActualWidth, 1);
        var height = Math.Max(ActualHeight, 1);
        var accent = ColorFromHex(settings.AccentColor, Colors.LightBlue);
        var speed = settings.ParticleSpeed switch
        {
            "Slow" => 1.65,
            "Fast" => 0.58,
            _ => 1.0
        };

        for (var i = 0; i < Math.Clamp(settings.ParticleCount, 0, 100); i++)
        {
            var size = _random.Next(2, 6);
            var particle = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)_random.Next(50, 135), accent.R, accent.G, accent.B)),
                Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 2 }
            };

            var x = _random.NextDouble() * width;
            var y = _random.NextDouble() * height;
            System.Windows.Controls.Canvas.SetLeft(particle, x);
            System.Windows.Controls.Canvas.SetTop(particle, y);
            ParticleCanvas.Children.Add(particle);

            var duration = TimeSpan.FromMilliseconds(_random.Next(4200, 8500) * speed);
            var drift = new DoubleAnimation(y, y - _random.Next(35, 150), duration)
            {
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true,
                BeginTime = TimeSpan.FromMilliseconds(_random.Next(0, 1200))
            };
            particle.BeginAnimation(System.Windows.Controls.Canvas.TopProperty, drift);

            var pulse = new DoubleAnimation(0.18, 0.82, TimeSpan.FromMilliseconds(_random.Next(1800, 4200) * speed))
            {
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            particle.BeginAnimation(OpacityProperty, pulse);
        }
    }

    private void Dismiss_Click(object sender, RoutedEventArgs e) => Close();

    protected override void OnClosed(EventArgs e)
    {
        _closeTimer.Stop();
        BackgroundVideo.Stop();
        base.OnClosed(e);
    }
}