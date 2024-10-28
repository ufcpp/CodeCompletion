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

        var f = Factory.Create(typeof(A));

        var buffer = new TextBuffer();

        TextInput += (sender, e) =>
        {
            buffer.Insert(e.Text);
            System.Diagnostics.Debug.WriteLine($"ti: {buffer}");
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

                var (t, p) = buffer.GetPosition();
                System.Diagnostics.Debug.WriteLine($"cursor: {buffer.Cursor} token: {t} pos: {p} move: {move}");
            }
        };
    }
}