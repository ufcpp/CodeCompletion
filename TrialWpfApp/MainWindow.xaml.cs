using CodeCompletion.Semantics;
using CodeCompletion.Text;
using System.Windows;
using System.Windows.Input;
using TrialWpfApp.Models;

namespace TrialWpfApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var buffer = new TextBuffer();
        var model = new SemanticModel(typeof(A), buffer);

        void show()
        {
            model.Refresh();
            var (t, p) = buffer.GetPosition();
            System.Diagnostics.Debug.WriteLine($"""
text: {buffer}
cursor: {buffer.Cursor} token: {t} pos: {p}
nodes: {string.Join(", ", model.Nodes)}
candidates: {string.Join(", ", model.GetCandidates().Select(x => x.Text))}
""");
        }

        TextInput += (sender, e) =>
        {
            if (e.Text.Length == 0) return;
            if (char.GetUnicodeCategory(e.Text[0]) == System.Globalization.UnicodeCategory.Control) return;

            buffer.Insert(e.Text);
            show();
        };

        KeyDown += (sender, e) =>
        {
            var ctrl = Keyboard.GetKeyStates(Key.LeftCtrl).HasFlag(KeyStates.Down)
                || Keyboard.GetKeyStates(Key.RightCtrl).HasFlag(KeyStates.Down);

            var move = (ctrl, e.Key) switch
            {
                (false, Key.Left) => CursorMove.Back,
                (false, Key.Right) => CursorMove.Forward,
                (true, Key.Left) => CursorMove.StartToken,
                (true, Key.Right) => CursorMove.EndToken,
                (_, Key.Home) => CursorMove.StartText,
                (_, Key.End) => CursorMove.EndText,
                _ => default,
            };

            if (move != 0)
            {
                buffer.Move(move);
                System.Diagnostics.Debug.WriteLine($"move: {move}");
                show();
                return;
            }

            move = (ctrl, e.Key) switch
            {
                (false, Key.Back) => CursorMove.Back,
                (false, Key.Delete) => CursorMove.Forward,
                (true, Key.Back) => CursorMove.StartToken,
                (true, Key.Delete) => CursorMove.EndToken,
                _ => default,
            };

            if (move != 0)
            {
                buffer.Remove(move);
                System.Diagnostics.Debug.WriteLine($"remove: {move}");
                show();
            }
        };
    }
}