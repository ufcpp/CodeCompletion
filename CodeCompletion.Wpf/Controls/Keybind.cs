using CodeCompletion.Text;
using CodeCompletion.ViewModels;
using System.Windows.Input;

namespace CodeCompletion.Controls;

internal static class Keybind
{
    public static (bool handled, bool invalidates) Handle(Key key, bool ctrl, ViewModel viewModel)
    {
        if (GetHandler(key, ctrl) is not { } handler) return (false, false);
        return (true, handler(viewModel));
    }

    const int CtrlOffset = 256;

    public static Func<ViewModel, bool>? GetHandler(Key key, bool ctrl)
    {
        var k = (int)key;
        if (ctrl) k += CtrlOffset;
        return _table.TryGetValue(k, out var v) ? v : null;
    }

    private static readonly Dictionary<int, Func<ViewModel, bool>> _table = CreateTable();

    private static Dictionary<int, Func<ViewModel, bool>> CreateTable()
    {
        var table = new Dictionary<int, Func<ViewModel, bool>>();

        table.Neutral(Key.Enter, static vm => vm.Complete());
        table.Ctrl(Key.Enter, static vm => { vm.Filter(); return false; });

        table.Either(Key.Down, static vm => vm.Next());
        table.Either(Key.Up, static vm => vm.Prev());

        table.Neutral(Key.Left, static vm => vm.Texts.Move(CursorMove.Back));
        table.Neutral(Key.Right, static vm => vm.Texts.Move(CursorMove.Forward));
        table.Ctrl(Key.Left, static vm => vm.Texts.Move(CursorMove.StartToken));
        table.Ctrl(Key.Right, static vm => vm.Texts.Move(CursorMove.EndToken));
        table.Either(Key.Home, static vm => vm.Texts.Move(CursorMove.StartText));
        table.Either(Key.End, static vm => vm.Texts.Move(CursorMove.EndText));

        table.Neutral(Key.Back, static vm => vm.Texts.Remove(CursorMove.Back));
        table.Neutral(Key.Delete, static vm => vm.Texts.Remove(CursorMove.Forward));
        table.Ctrl(Key.Back, static vm => vm.Texts.Remove(CursorMove.StartToken));
        table.Ctrl(Key.Delete, static vm => vm.Texts.Remove(CursorMove.EndToken));

        return table;
    }

    private static void Add(this Dictionary<int, Func<ViewModel, bool>> table, int key, Action<ViewModel> action) => table.Add(key, action.ReturnTrue);
    private static void Neutral(this Dictionary<int, Func<ViewModel, bool>> table, Key key, Action<ViewModel> action) => table.Add((int)key, action);
    private static void Ctrl(this Dictionary<int, Func<ViewModel, bool>> table, Key key, Action<ViewModel> action) => table.Add(CtrlOffset + (int)key, action);
    private static void Either(this Dictionary<int, Func<ViewModel, bool>> table, Key key, Action<ViewModel> action) { table.Neutral(key, action); table.Ctrl(key, action); }
    private static void Neutral(this Dictionary<int, Func<ViewModel, bool>> table, Key key, Func<ViewModel, bool> func) => table.Add((int)key, func);
    private static void Ctrl(this Dictionary<int, Func<ViewModel, bool>> table, Key key, Func<ViewModel, bool> func) => table.Add(CtrlOffset + (int)key, func);

    private static bool ReturnTrue<T>(this Action<T> action, T arg)
    {
        action(arg);
        return true;
    }
}
