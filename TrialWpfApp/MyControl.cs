using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace TrialWpfApp;

public class MyControl : TextBlock
{
    public MyControl()
    {
        Focusable = true;
        Margin = new(5);

        var drawingBrush = new DrawingBrush
        {
            Stretch = Stretch.None,
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top,
        };
        Background = drawingBrush;

        Loaded += (_, _) =>
        {
            Height = Math.Ceiling(FontFamily.LineSpacing * FontSize);
        };

        void show(ViewModel vm)
        {
            vm.Refresh();

            var textDest = new DrawingGroup();
            DrawingContext dc = textDest.Open();

            Render(vm, dc);

            drawingBrush.Drawing = textDest;

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

            var ctrl = Keyboard.GetKeyStates(Key.LeftCtrl).HasFlag(KeyStates.Down)
                || Keyboard.GetKeyStates(Key.RightCtrl).HasFlag(KeyStates.Down);

            var (handled, invalidates) = Keybind.Handle(e.Key, ctrl, vm);
            if (invalidates) show(vm);
            if (handled) e.Handled = true;
        };
    }

    private void Render(ViewModel vm, DrawingContext dc)
    {
        var formatter = TextFormatter.Create();
        var prop = new GenericTextRunProperties(FontSize, FontSize, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch));
        var textSource = new MyTextSource(vm.Semantics, prop);
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
                new ParaProp(Height, prop),
                null);

            // Draw the formatted text into the drawing context.
            myTextLine.Draw(dc, linePosition, InvertAxes.None);

            // Update the index position in the text store.
            textStorePosition += myTextLine.Length;

            // Update the line position coordinate for the displayed line.
            linePosition.Y += myTextLine.Height;
        }

        dc.Close();
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
