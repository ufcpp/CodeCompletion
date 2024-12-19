using CodeCompletion.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CodeCompletion.Controls;

public static class CopyAndPaste
{
    public static void BindCopyAndPaste(this Control e)
    {
        void add(RoutedUICommand command, Action<ViewModel> action)
        {
            e.ContextMenu.Items.Add(new MenuItem { Command = command });

            foreach (var key in command.InputGestures.OfType<KeyGesture>())
            {
                e.InputBindings.Add(new KeyBinding(command, key));
            }

            e.CommandBindings.Add(new CommandBinding(command, (sender, arg) =>
            {
                if (sender is not FrameworkElement e) return;
                if (e.DataContext is not ViewModel vm) return;
                action(vm);
                arg.Handled = true;
            }));
        }

        e.ContextMenu = new();

        add(ApplicationCommands.Copy, vm => Clipboard.SetData(DataFormats.Text, vm.Texts.ToString()));

        add(CopyAllCommand, vm => Clipboard.SetData(DataFormats.Text, vm.History.ToString()));

        add(ApplicationCommands.Paste, vm =>
        {
            if (Clipboard.GetData(DataFormats.Text) is not string s) return;
            vm.Reset(s);
        });
    }

    private static readonly RoutedUICommand CopyAllCommand = new("履歴コピー", "Copy All", typeof(CopyAndPaste), [new KeyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Shift)]);
}
