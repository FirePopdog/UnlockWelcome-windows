using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using UnlockWelcome.Models;

namespace UnlockWelcome.Views;

public partial class WelcomeWindow : Window
{
    private readonly DispatcherTimer _closeTimer = new();
    private readonly Random _random = new();

    public WelcomeWindow(bool preview = false)
    {
        InitializeComponent();
        Greeting.Text = AppSettings.Current.Greeting;
        Subtitle.Text = AppSettings.Current.Subtitle;
        DateText.Text = DateTime.Now.ToString("dddd, MMMM d  •  HH:mm", System.Globalization.CultureInfo.InvariantCulture);
        Loaded += (_, _) => PlayAnimation();

        if (AppSettings.Current.DurationSeconds > 0)
        {
            _closeTimer.Interval = TimeSpan.FromSeconds(AppSettings.Current.DurationSeconds);
            _closeTimer.Tick += (_, _) => Close();
            _closeTimer.Start();
        }
    }

    private void PlayAnimation()
    {
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(900))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Root.BeginAnimation(OpacityProperty, fadeIn);

        AnimateElement(Eyebrow, 0, 1, 450);
        AnimateElement(Greeting, 180, 1, 850);
        AnimateElement(Subtitle, 520, 1, 700);
        AnimateElement(DateText, 850, 1, 650);
        AnimateElement(AccentLine, 650, 1, 700);

        var lineWidth = new DoubleAnimation(0, 150, TimeSpan.FromMilliseconds(850))
        {
            BeginTime = TimeSpan.FromMilliseconds(650),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        AccentLine.BeginAnimation(WidthProperty, lineWidth);

        CreateParticles();
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

    private void CreateParticles()
    {
        var width = ActualWidth;
        var height = ActualHeight;

        for (var i = 0; i < 34; i++)
        {
            var size = _random.Next(2, 5);
            var particle = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)_random.Next(45, 125), 170, 194, 255)),
                Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 2 }
            };

            var x = _random.NextDouble() * Math.Max(width, 1);
            var y = _random.NextDouble() * Math.Max(height, 1);
            System.Windows.Controls.Canvas.SetLeft(particle, x);
            System.Windows.Controls.Canvas.SetTop(particle, y);
            ParticleCanvas.Children.Add(particle);

            var drift = new DoubleAnimation(y, y - _random.Next(35, 150), TimeSpan.FromSeconds(_random.Next(5, 10)))
            {
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true,
                BeginTime = TimeSpan.FromMilliseconds(_random.Next(0, 1200))
            };
            particle.BeginAnimation(System.Windows.Controls.Canvas.TopProperty, drift);

            var pulse = new DoubleAnimation(0.15, 0.8, TimeSpan.FromSeconds(_random.Next(2, 5)))
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
        base.OnClosed(e);
    }
}