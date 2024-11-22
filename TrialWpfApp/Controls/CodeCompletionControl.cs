using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace TrialWpfApp.Controls;

public partial class CodeCompletionControl : ContentControl
{
    private readonly TextView _text;
    private readonly Line _caret;
    private readonly Storyboard _caretBlink;

    public CodeCompletionControl()
    {
        _text = new(this);
        _caret = new() { StrokeThickness = 1, Stroke = Brushes.Black };
        _caretBlink = Blink(_caret);
        Content = new Canvas
        {
            Children = { _text, _caret },
        };

        Focusable = true;
        Margin = new(5);

        Loaded += (_, _) => UpdateTextProperties(true);
        DataContextChanged += (_, _) => UpdateViewModel();

        void show(ViewModel vm)
        {
            vm.Refresh();
            _text.InvalidateVisual();
            ShowDiag(vm);
        }

        //todo: InputBindings KeyBinding でやった方がいい？
        TextInput += (sender, e) =>
        {
            if (DataContext is not ViewModel vm) return;

            if (e.Text.Length == 0) return;
            if (char.GetUnicodeCategory(e.Text[0]) == System.Globalization.UnicodeCategory.Control) return;

            vm.Texts.Insert(e.Text);
            show(vm);
        };

        KeyDown += (sender, e) =>
        {
            if (DataContext is not ViewModel vm) return;

            var (handled, invalidates) = Keybind.Handle(e.Key, CtrlKeyDown, vm);
            if (invalidates) show(vm);
            if (handled) e.Handled = true;
        };
    }

    private static bool CtrlKeyDown
        => Keyboard.GetKeyStates(Key.LeftCtrl).HasFlag(KeyStates.Down)
        || Keyboard.GetKeyStates(Key.RightCtrl).HasFlag(KeyStates.Down);

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == FontSizeProperty
            || e.Property == FontFamilyProperty)
        {
            UpdateTextProperties(true);
            InvalidateVisual();
        }
        else if (e.Property == FontStyleProperty
            || e.Property == FontWeightProperty
            || e.Property == FontStretchProperty)
        {
            UpdateTextProperties();
            InvalidateVisual();
        }
    }

    private CommonTextProperties _textProperties;
    internal CodeCompletionTextSource? TextSource { get; private set; }

    private void UpdateTextProperties(bool updatesHeight = false)
    {
        if (updatesHeight) Height = Math.Ceiling(FontFamily.LineSpacing * FontSize); // 改行を想定してない
        _textProperties = new CommonTextProperties(FontSize, FontFamily, FontStyle, FontWeight, FontStretch);
        UpdateViewModel();
    }

    private void UpdateViewModel()
    {
        TextSource = DataContext is ViewModel vm && _textProperties is { } prop
            ? new(vm.Semantics, prop, Height) // 改行を想定してない
            : null;
    }

    private static void ShowDiag(ViewModel vm)
    {
        var buffer = vm.Texts;
        var (t, p) = buffer.GetPosition();
        System.Diagnostics.Debug.WriteLine($"""
cursor: {buffer.Cursor} token: {t} pos: {p}
nodes: {string.Join(", ", vm.Semantics.Nodes)}
candidates: {string.Join(", ", vm.Candidates.Select(x => x.Text))} (selected: {vm.SelectedCandidateIndex})

""");
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

    internal void UpdateCaret(double x)
    {
        _caret.X1 = x;
        _caret.Y1 = 0;
        _caret.X2 = x;
        _caret.Y2 = Height; // 改行を想定してない

        _caretBlink.Seek(default);
    }
}
