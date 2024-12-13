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
                new MenuItem { Header = "Copy All", Command = CopyAllCommand },
                new MenuItem { Header = "Paste", Command = ApplicationCommands.Paste },
            }
        };

        e.InputBindings.Add(new KeyBinding(ApplicationCommands.Copy, Key.C, ModifierKeys.Control));
        e.InputBindings.Add(new KeyBinding(CopyAllCommand, Key.C, ModifierKeys.Control | ModifierKeys.Shift));
        e.InputBindings.Add(new KeyBinding(ApplicationCommands.Paste, Key.V, ModifierKeys.Control));

        e.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, (sender, arg) =>
        {
            if (sender is not FrameworkElement e) return;
            if (e.DataContext is not ViewModel vm) return;
            Clipboard.SetData(DataFormats.Text, vm.Texts.ToString());
        }));

        e.CommandBindings.Add(new CommandBinding(CopyAllCommand, (sender, arg) =>
        {
            if (sender is not FrameworkElement e) return;
            if (e.DataContext is not ViewModel vm) return;
            Clipboard.SetData(DataFormats.Text, vm.History.ToString());
        }));

        e.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, (sender, arg) =>
        {
            if (sender is not FrameworkElement e) return;
            if (e.DataContext is not ViewModel vm) return;
            if (Clipboard.GetData(DataFormats.Text) is not string s) return;
            vm.Reset(s);
        }));
    }

    private static readonly RoutedUICommand CopyAllCommand = new RoutedUICommand("Copy All", "Copy All", typeof(CopyAndPaste), new() { new KeyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Shift) });
}
