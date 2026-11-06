using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using UnlockWelcome.Models;
using UnlockWelcome.Services;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using Point = System.Windows.Point;
using Path = System.IO.Path;
using Forms = System.Windows.Forms;

namespace UnlockWelcome.Views;

public partial class WelcomeWindow : Window
{
    private readonly DispatcherTimer _closeTimer = new();
    private readonly DispatcherTimer _loadingTimer = new();
    private readonly DispatcherTimer _typewriterTimer = new();
    private readonly Random _random = new();
    private GifPlayback? _gifPlayback;
    private string _fullGreeting = "";
    private int _typewriterIndex;

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

        _loadingTimer.Tick += (_, _) => ShowWelcomeContent(settings);
        _closeTimer.Tick += (_, _) => Close();
        _typewriterTimer.Interval = TimeSpan.FromMilliseconds(42);
        _typewriterTimer.Tick += Typewriter_Tick;
        Loaded += (_, _) => StartExperience(settings);
    }

    private void ApplySettings(AppSettings settings)
    {
        _fullGreeting = settings.Greeting;
        Greeting.Text = settings.Greeting;
        Subtitle.Text = settings.Subtitle;
        Greeting.FontFamily = new FontFamily(string.IsNullOrWhiteSpace(settings.FontFamilyName) ? "Segoe UI" : settings.FontFamilyName);
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

        LoadingTitleText.Text = string.IsNullOrWhiteSpace(settings.LoadingTitle)
            ? "WELCOME TO YOUR SPACE" : settings.LoadingTitle;
        LoadingSubtitleText.Text = settings.LoadingSubtitle;
        LoadingStatusText.Text = settings.LoadingStatusText;
        LoadingSubtitleText.Visibility = string.IsNullOrWhiteSpace(settings.LoadingSubtitle)
            ? Visibility.Collapsed : Visibility.Visible;
        LoadingStatusText.Visibility = string.IsNullOrWhiteSpace(settings.LoadingStatusText)
            ? Visibility.Collapsed : Visibility.Visible;
        LoadingProgressTrack.Visibility = settings.ShowLoadingProgress
            ? Visibility.Visible : Visibility.Collapsed;
        LoadingPanel.Visibility = settings.EnableLoadingScreen ? Visibility.Visible : Visibility.Collapsed;
        ContentPanel.Opacity = settings.EnableLoadingScreen ? 0 : 1;

        LoadBackgroundMedia(settings);
    }

    private void LoadBackgroundMedia(AppSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.BackgroundVideoPath) &&
            File.Exists(settings.BackgroundVideoPath))
        {
            try
            {
                BackgroundVideo.IsMuted = settings.MuteBackgroundVideo;
                BackgroundVideo.Source = new Uri(settings.BackgroundVideoPath, UriKind.Absolute);
                BackgroundVideo.Visibility = Visibility.Visible;
                BackgroundOverlay.Fill = new SolidColorBrush(Color.FromArgb(125, 0, 0, 0));
                return;
            }
            catch
            {
                BackgroundVideo.Visibility = Visibility.Collapsed;
            }
        }

        if (!string.IsNullOrWhiteSpace(settings.BackgroundImagePath) &&
            File.Exists(settings.BackgroundImagePath))
        {
            try
            {
                if (Path.GetExtension(settings.BackgroundImagePath).Equals(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    BackgroundGifImage.Visibility = Visibility.Visible;
                    _gifPlayback = new GifPlayback(BackgroundGifImage, settings.BackgroundImagePath);
                }
                else
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = new Uri(settings.BackgroundImagePath, UriKind.Absolute);
                    image.EndInit();
                    image.Freeze();
                    BackgroundImage.Source = image;
                    BackgroundImage.Visibility = Visibility.Visible;
                }

                BackgroundOverlay.Fill = new SolidColorBrush(Color.FromArgb(105, 0, 0, 0));
            }
            catch
            {
                BackgroundImage.Visibility = Visibility.Collapsed;
                BackgroundGifImage.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void StartExperience(AppSettings settings)
    {
        Root.Opacity = 0;
        Root.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(650))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        });

        CreateAmbientRings(settings);
        if (settings.ShowParticles && settings.ParticleCount > 0)
            CreateParticles(settings);

        if (settings.EnableLoadingScreen)
        {
            LoadingPanel.Visibility = Visibility.Visible;
            LoadingPanel.Opacity = 0;
            LoadingPanel.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(700)));
            AnimateLoadingProgress();

            if (settings.LoadingDurationSeconds > 0)
            {
                _loadingTimer.Interval = TimeSpan.FromSeconds(Math.Clamp(settings.LoadingDurationSeconds, 1, 30));
                _loadingTimer.Start();
            }
        }
        else
        {
            ShowWelcomeContent(settings);
        }
    }

    private void AnimateLoadingProgress()
    {
        if (LoadingProgressTrack.Visibility != Visibility.Visible)
            return;

        var travel = Math.Max(LoadingProgressTrack.ActualWidth, 380);
        var widthAnimation = new DoubleAnimation(45, 150, TimeSpan.FromMilliseconds(950))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        LoadingProgressIndicator.BeginAnimation(WidthProperty, widthAnimation);

        LoadingProgressIndicator.BeginAnimation(MarginProperty, new ThicknessAnimation
        {
            From = new Thickness(0, 0, travel - 150, 0),
            To = new Thickness(travel - 150, 0, 0, 0),
            Duration = TimeSpan.FromMilliseconds(1800),
            RepeatBehavior = RepeatBehavior.Forever,
            AutoReverse = true
        });
    }

    private void ShowWelcomeContent(AppSettings settings)
    {
        _loadingTimer.Stop();

        if (LoadingPanel.Visibility == Visibility.Visible)
        {
            var fadeOut = new DoubleAnimation(LoadingPanel.Opacity, 0, TimeSpan.FromMilliseconds(650))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            fadeOut.Completed += (_, _) => LoadingPanel.Visibility = Visibility.Collapsed;
            LoadingPanel.BeginAnimation(OpacityProperty, fadeOut);
        }
        else
        {
            LoadingPanel.Visibility = Visibility.Collapsed;
        }

        DismissButton.Content = "關閉  ×";
        PlayWelcomeAnimations(settings);

        if (settings.DurationSeconds > 0)
        {
            _closeTimer.Interval = TimeSpan.FromSeconds(Math.Clamp(settings.DurationSeconds, 1, 120));
            _closeTimer.Start();
        }
    }

    private void PlayWelcomeAnimations(AppSettings settings)
    {
        var style = settings.AnimationStyle;
        ContentPanel.Opacity = 0;
        ContentPanel.Effect = null;
        ZoomTransform.ScaleX = 1;
        ZoomTransform.ScaleY = 1;
        SlideTransform.Y = 0;

        ContentPanel.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(850))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        });

        switch (style)
        {
            case "Zoom":
                ZoomTransform.ScaleX = 0.78;
                ZoomTransform.ScaleY = 0.78;
                var zoom = new DoubleAnimation(0.78, 1, TimeSpan.FromMilliseconds(1300))
                {
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.25 }
                };
                ZoomTransform.BeginAnimation(ScaleTransform.ScaleXProperty, zoom);
                ZoomTransform.BeginAnimation(ScaleTransform.ScaleYProperty, zoom);
                break;

            case "Slide":
                SlideTransform.Y = 95;
                SlideTransform.BeginAnimation(TranslateTransform.YProperty,
                    new DoubleAnimation(95, 0, TimeSpan.FromMilliseconds(1050))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    });
                break;

            case "Blur":
                var blur = new System.Windows.Media.Effects.BlurEffect { Radius = 26 };
                ContentPanel.Effect = blur;
                blur.BeginAnimation(System.Windows.Media.Effects.BlurEffect.RadiusProperty,
                    new DoubleAnimation(26, 0, TimeSpan.FromMilliseconds(1350))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    });
                break;

            case "Bounce":
                SlideTransform.Y = -95;
                SlideTransform.BeginAnimation(TranslateTransform.YProperty,
                    new DoubleAnimation(-95, 0, TimeSpan.FromMilliseconds(1250))
                    {
                        EasingFunction = new BounceEase { Bounces = 2, Bounciness = 2.4, EasingMode = EasingMode.EaseOut }
                    });
                break;

            case "Typewriter":
                _typewriterTimer.Stop();
                _typewriterIndex = 0;
                Greeting.Text = "";
                AnimateElement(Greeting, 180, 1, 250);
                _typewriterTimer.Start();
                break;

            case "Glitch":
                SlideTransform.BeginAnimation(TranslateTransform.YProperty,
                    new DoubleAnimation(-3, 0, TimeSpan.FromMilliseconds(420)));
                break;
        }

        AnimateElement(Eyebrow, 80, 1, 520);
        if (style != "Typewriter" && style != "Glitch")
            AnimateElement(Greeting, 180, 1, 850);
        AnimateElement(Subtitle, 420, 1, 780);
        AnimateElement(AccentLine, 620, 1, 680);
        AnimateElement(DateText, 800, 1, 650);
        AccentLine.BeginAnimation(WidthProperty, new DoubleAnimation(0, 170, TimeSpan.FromMilliseconds(1000))
        {
            BeginTime = TimeSpan.FromMilliseconds(620),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        });

        if (style == "Glitch")
        {
            var flicker = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(950),
                FillBehavior = FillBehavior.HoldEnd
            };
            flicker.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.15, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))));
            flicker.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(90))));
            flicker.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.25, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150))));
            flicker.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(240))));
            flicker.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.65, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(340))));
            flicker.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(480))));
            Greeting.BeginAnimation(OpacityProperty, flicker);
        }

        GlowOrb.BeginAnimation(OpacityProperty, new DoubleAnimation(0.12, 0.32, TimeSpan.FromSeconds(2.5))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever
        });
    }

    private void Typewriter_Tick(object? sender, EventArgs e)
    {
        if (_typewriterIndex >= _fullGreeting.Length)
        {
            _typewriterTimer.Stop();
            return;
        }

        _typewriterIndex = Math.Min(_typewriterIndex + 1, _fullGreeting.Length);
        Greeting.Text = _fullGreeting[.._typewriterIndex];
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

    private static Brush BrushFromHex(string value, string fallback)
    {
        try
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
        }
        catch
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(fallback));
        }
    }

    private static Color ColorFromHex(string value, Color fallback)
    {
        try
        {
            return (Color)ColorConverter.ConvertFromString(value);
        }
        catch
        {
            return fallback;
        }
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

    private void CreateAmbientRings(AppSettings settings)
    {
        var color = ColorFromHex(settings.AccentColor, Colors.LightBlue);
        for (var i = 0; i < 3; i++)
        {
            var ring = new Ellipse
            {
                Width = 120,
                Height = 120,
                Stroke = new SolidColorBrush(Color.FromArgb(70, color.R, color.G, color.B)),
                StrokeThickness = 1,
                Fill = Brushes.Transparent,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(0.2, 0.2),
                IsHitTestVisible = false
            };
            System.Windows.Controls.Canvas.SetLeft(ring, (ActualWidth - 120) / 2);
            System.Windows.Controls.Canvas.SetTop(ring, (ActualHeight - 120) / 2);
            AmbientCanvas.Children.Add(ring);

            var scale = (ScaleTransform)ring.RenderTransform;
            var delay = TimeSpan.FromMilliseconds(i * 900);
            var expand = new DoubleAnimation(0.2, 4.4, TimeSpan.FromSeconds(4.5))
            {
                BeginTime = delay,
                RepeatBehavior = RepeatBehavior.Forever
            };
            var fade = new DoubleAnimation(0.45, 0, TimeSpan.FromSeconds(4.5))
            {
                BeginTime = delay,
                RepeatBehavior = RepeatBehavior.Forever
            };
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, expand);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, expand);
            ring.BeginAnimation(OpacityProperty, fade);
        }
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
                Fill = new SolidColorBrush(Color.FromArgb((byte)_random.Next(50, 135), accent.R, accent.G, accent.B)),
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

    private void Dismiss_Click(object sender, RoutedEventArgs e)
    {
        if (LoadingPanel.Visibility == Visibility.Visible)
        {
            ShowWelcomeContent(AppSettings.Current);
            return;
        }

        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _closeTimer.Stop();
        _loadingTimer.Stop();
        _typewriterTimer.Stop();
        _gifPlayback?.Dispose();
        BackgroundVideo.Stop();
        base.OnClosed(e);
    }
}