using CodeCompletion.Text;
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

        var table = new Dictionary<int, Action<ViewModel>>();

        void neutral(Key key, Action<ViewModel> action) => table[(int)key] = action;
        void ctrl(Key key, Action<ViewModel> action) => table[256 + (int)key] = action;
        void either(Key key, Action<ViewModel> action) { neutral(key, action); ctrl(key, action); }

        neutral(Key.Enter, vm =>
        {
            if (vm.Complete())
            {
                show(vm);
            }
        });

        ctrl(Key.Enter, vm => vm.Filter());

        either(Key.Down, vm => { vm.Next(); show(vm); });
        either(Key.Up, vm => { vm.Prev(); show(vm); });

        void move(ViewModel vm, CursorMove m)
        {
            vm.Texts.Move(m);
            System.Diagnostics.Debug.WriteLine($"move: {m}");
            show(vm);
        }

        neutral(Key.Left, vm => move(vm, CursorMove.Back));
        neutral(Key.Right, vm => move(vm, CursorMove.Forward));
        ctrl(Key.Left, vm => move(vm, CursorMove.StartToken));
        ctrl(Key.Right, vm => move(vm, CursorMove.EndToken));
        either(Key.Home, vm => move(vm, CursorMove.StartText));
        either(Key.End, vm => move(vm, CursorMove.EndText));

        void remove(ViewModel vm, CursorMove m)
        {
            vm.Texts.Remove(m);
            System.Diagnostics.Debug.WriteLine($"remove: {m}");
            show(vm);
        }

        neutral(Key.Back, vm => remove(vm, CursorMove.Back));
        neutral(Key.Delete, vm => remove(vm, CursorMove.Forward));
        ctrl(Key.Back, vm => remove(vm, CursorMove.StartToken));
        ctrl(Key.Delete, vm => remove(vm, CursorMove.EndToken));

        KeyDown += (sender, e) =>
        {
            if (DataContext is not ViewModel vm) return;

            var key = (int)e.Key;

            if (Keyboard.GetKeyStates(Key.LeftCtrl).HasFlag(KeyStates.Down)
                || Keyboard.GetKeyStates(Key.RightCtrl).HasFlag(KeyStates.Down)) key += 256;

            if (table.TryGetValue(key, out var action))
            {
                action(vm);
                e.Handled = true;
            }
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
