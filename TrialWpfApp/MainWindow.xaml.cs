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

        DataContext = SampleData.Data;

        var buffer = new TextBuffer();
        var model = new SemanticModel(typeof(A), buffer);
        IReadOnlyList<Candidate> candidates = [];
        int selectedCandidateIndex = 0;

        void show()
        {
            model.Refresh();
            candidates = model.GetCandidates();

            var (t, p) = buffer.GetPosition();
            System.Diagnostics.Debug.WriteLine($"""
text: {buffer}
cursor: {buffer.Cursor} token: {t} pos: {p}
nodes: {string.Join(", ", model.Nodes)}
candidates: {string.Join(", ", candidates.Select(x => x.Text))} (selected: {selectedCandidateIndex})

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

            if (e.Key == Key.Enter) // Tab も？
            {
                if (ctrl)
                {
                    var filter = model.Emit();

                    if (filter is null)
                    {
                        System.Diagnostics.Debug.WriteLine("filter OFF");
                        DataContext = SampleData.Data;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("filter ON");
                        DataContext = SampleData.Data.Where(filter).ToList();
                    }
                    return;
                }

                // 補完候補確定。
                if (candidates.ElementAtOrDefault(selectedCandidateIndex) is { } c)
                {
                    buffer.Replace(c.Text);
                    selectedCandidateIndex = 0;
                    show();
                }
                return;
            }

            if (e.Key == Key.Down)
            {
                // 補完候補を1個下に。
                selectedCandidateIndex++;
                if (selectedCandidateIndex >= candidates.Count) selectedCandidateIndex = 0;
                show();
                return;
            }

            if (e.Key == Key.Up)
            {
                // 補完候補を1個上に。
                selectedCandidateIndex--;
                if (selectedCandidateIndex < 0) selectedCandidateIndex = candidates.Count - 1;
                show();
                return;
            }

            // カーソル移動。
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

            // 文字削除。
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