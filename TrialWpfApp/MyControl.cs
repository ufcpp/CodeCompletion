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

            if (table.TryGetValue(key, out var action)) action(vm);
        };
    }
}
