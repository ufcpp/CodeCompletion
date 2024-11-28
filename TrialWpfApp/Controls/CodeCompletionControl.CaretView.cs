using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace TrialWpfApp.Controls;

partial class CodeCompletionControl
{
    private class CaretView
    {
        public Line Line { get; }
        private readonly Storyboard _blink;

        public CaretView()
        {
            Line = new() { StrokeThickness = 1, Stroke = Brushes.Black };
            _blink = Blink(Line);
        }

        private static Storyboard Blink(UIElement x)
        {
            var a = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
            {
                new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))),
                new DiscreteDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500))),
            }
            };

            var s = new Storyboard
            {
                Duration = TimeSpan.FromMilliseconds(1000),
                RepeatBehavior = RepeatBehavior.Forever,
                Children = { a },
            };

            Storyboard.SetTarget(a, x);
            Storyboard.SetTargetProperty(a, new PropertyPath(OpacityProperty));
            s.Begin();

            return s;
        }

        internal void Update(double x, double height)
        {
            Line.X1 = x;
            Line.Y1 = 0;
            Line.X2 = x;
            Line.Y2 = height; // 改行を想定してない

            _blink.Seek(default);
        }
    }
}
