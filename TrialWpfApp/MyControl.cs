using CodeCompletion.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace TrialWpfApp;

public class MyControl : TextBlock
{
    public MyControl()
    {
        Focusable = true;
        Height = 30;

        void show(ViewModel vm)
        {
            vm.Refresh();

            var buffer = vm.Texts;

            //todo: 変化した Token に対応する部分だけ更新できないか。
            Inlines.Clear();
            foreach (var token in vm.Texts.Tokens)
            {
                //todo: Node のタイプで色分け。
                Inlines.Add(token.Span.ToString());
            }

            //todo: カレット表示。

            //todo: 補完候補をポップアップ。

            var (t, p) = buffer.GetPosition();
            System.Diagnostics.Debug.WriteLine($"""
cursor: {buffer.Cursor} token: {t} pos: {p}
nodes: {string.Join(", ", vm.Semantics.Nodes)}
candidates: {string.Join(", ", vm.Candidates.Select(x => x.Text))} (selected: {vm.SelectedCandidateIndex})

""");
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
                    show(vm);
                }
                return;
            }

            if (e.Key == Key.Down)
            {
                vm.Next();
                show(vm);
                return;
            }

            if (e.Key == Key.Up)
            {
                vm.Prev();
                show(vm);
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
                show(vm);
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
                show(vm);
            }
        };
    }
}
