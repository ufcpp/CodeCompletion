using CodeCompletion.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CodeCompletion.Controls;

public static class CopyAndPaste
{
    public static void BindCopyAndPaste(this Control e)
    {
        e.ContextMenu = new()
        {
            Items =
            {
                new MenuItem { Header = "Copy", Command = ApplicationCommands.Copy },
                new MenuItem { Header = "Paste", Command = ApplicationCommands.Paste },
            }
        };

        e.InputBindings.Add(new KeyBinding(ApplicationCommands.Copy, Key.C, ModifierKeys.Control));
        e.InputBindings.Add(new KeyBinding(ApplicationCommands.Paste, Key.V, ModifierKeys.Control));

        e.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, (sender, arg) =>
        {
            if (sender is not FrameworkElement e) return;
            if (e.DataContext is not ViewModel vm) return;
            Clipboard.SetData(DataFormats.Text, vm.Texts.ToString());
        }));

        e.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, (sender, arg) =>
        {
            if (sender is not FrameworkElement e) return;
            if (e.DataContext is not ViewModel vm) return;
            if (Clipboard.GetData(DataFormats.Text) is not string s) return;
            vm.Reset(s);
        }));
    }
}
