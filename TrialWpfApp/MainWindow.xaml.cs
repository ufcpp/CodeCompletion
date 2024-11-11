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

        var vm = new ViewModel(SampleData.Data);
        DataContext = vm;

        void show()
        {
            vm.Refresh();

            var buffer = vm.Texts;
            var (t, p) = buffer.GetPosition();
            System.Diagnostics.Debug.WriteLine($"""
text: {buffer}
cursor: {buffer.Cursor} token: {t} pos: {p}
nodes: {string.Join(", ", vm.Semantics.Nodes)}
candidates: {string.Join(", ", vm.Candidates.Select(x => x.Text))} (selected: {vm.SelectedCandidateIndex})

""");
        }

        TextInput += (sender, e) =>
        {
            if (e.Text.Length == 0) return;
            if (char.GetUnicodeCategory(e.Text[0]) == System.Globalization.UnicodeCategory.Control) return;

            vm.Texts.Insert(e.Text);
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
                    vm.Filter();
                    return;
                }

                // 補完候補確定。
                if (vm.Complete())
                {
                    show();
                }
                return;
            }

            if (e.Key == Key.Down)
            {
                vm.Next();
                show();
                return;
            }

            if (e.Key == Key.Up)
            {
                vm.Prev();
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
                vm.Texts.Move(move);
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
                vm.Texts.Remove(move);
                System.Diagnostics.Debug.WriteLine($"remove: {move}");
                show();
            }
        };
    }
}