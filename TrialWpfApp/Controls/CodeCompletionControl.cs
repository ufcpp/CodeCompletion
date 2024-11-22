using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace TrialWpfApp.Controls;

public class CodeCompletionControl : Control
{
    public CodeCompletionControl()
    {
        Focusable = true;
        Margin = new(5);

        Loaded += (_, _) => UpdateTextProperties(true);
        DataContextChanged += (_, _) => UpdateViewModel();

        void show(ViewModel vm)
        {
            vm.Refresh();
            InvalidateVisual();
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
    private CodeCompletionTextSource? _textSource;

    private void UpdateTextProperties(bool updatesHeight = false)
    {
        if (updatesHeight) Height = Math.Ceiling(FontFamily.LineSpacing * FontSize);
        _textProperties = new CommonTextProperties(FontSize, FontFamily, FontStyle, FontWeight, FontStretch);

        UpdateViewModel();
    }

    private void UpdateViewModel()
    {
        _textSource = DataContext is ViewModel vm && _textProperties is { } prop
            ? new(vm.Semantics, prop)
            : null;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (_textSource is not { } textSource) return;

        var formatter = TextFormatter.Create();
        var prop = _textProperties;
        var linePosition = new Point(0, 0);

        int textStorePosition = 0;
        while (textStorePosition < textSource.Length)
        {
            using var myTextLine = formatter.FormatLine(
                textSource,
                textStorePosition,
                96 * 6,
                new GenericTextParagraphProperties(Height, prop),
                null);

            myTextLine.Draw(drawingContext, linePosition, InvertAxes.None);
            textStorePosition += myTextLine.Length;
            linePosition.Y += myTextLine.Height;
        }
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
}
