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

    private void UpdateTextProperties(bool updatesHeight = false)
    {
        if (updatesHeight) Height = Math.Ceiling(FontFamily.LineSpacing * FontSize);
        _textProperties = new CommonTextProperties(FontSize, FontFamily, FontStyle, FontWeight, FontStretch);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (DataContext is not ViewModel vm) return;

        var formatter = TextFormatter.Create();
        var prop = _textProperties;
        var textSource = new CodeCompletionTextSource(vm.Semantics, prop);
        var linePosition = new Point(0, 0);

        //TextParagraphProperties

        int textStorePosition = 0;
        while (textStorePosition < vm.Texts.TotalLength)
        {
            // Create a textline from the text store using the TextFormatter object.
            using var myTextLine = formatter.FormatLine(
                textSource,
                textStorePosition,
                96 * 6,
                new GenericTextParagraphProperties(Height, prop),
                null);

            // Draw the formatted text into the drawing context.
            myTextLine.Draw(drawingContext, linePosition, InvertAxes.None);

            // Update the index position in the text store.
            textStorePosition += myTextLine.Length;

            // Update the line position coordinate for the displayed line.
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
