using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace TrialWpfApp.Controls;

public partial class CodeCompletionControl : ContentControl
{
    private readonly TextView _text;
    private readonly CaretView _caret;
    private readonly CandidateSelector _candidates;
    private readonly Popup _popup;

    public CodeCompletionControl()
    {
        _text = new(this);
        _caret = new();
        _candidates = new();

        _popup = new Popup
        {
            Child = _candidates,
            PlacementTarget = this,
            HorizontalOffset = 0,
            VerticalOffset = 0,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            PopupAnimation = PopupAnimation.Slide,
        };

        Content = new Canvas
        {
            Children = { _text, _caret.Line, _popup },
        };

        Focusable = true;
        Margin = new(5);

        Loaded += (_, _) => UpdateTextProperties(true);
        DataContextChanged += (_, args) => UpdateViewModel(args.OldValue, args.NewValue);

        //todo: InputBindings KeyBinding でやった方がいい？
        TextInput += (sender, e) =>
        {
            if (DataContext is not ViewModel vm) return;

            if (e.Text.Length == 0) return;
            if (char.GetUnicodeCategory(e.Text[0]) == System.Globalization.UnicodeCategory.Control) return;

            vm.Texts.Insert(e.Text);
            Show(vm);
        };

        KeyDown += (sender, e) =>
        {
            if (DataContext is not ViewModel vm) return;

            var (handled, invalidates) = Keybind.Handle(e.Key, CtrlKeyDown, vm);
            if (invalidates) Show(vm);
            if (handled) e.Handled = true;
        };
    }

    private void Show(ViewModel vm)
    {
        vm.Refresh();
        _candidates.Candidates = vm.Candidates;
        _candidates.SelectedIndex = vm.SelectedCandidateIndex;
        _candidates.Visibility = vm.Candidates.Any() ? Visibility.Visible : Visibility.Collapsed;
        _text.InvalidateVisual();
        ShowDiag(vm);
    }

    private static bool CtrlKeyDown
        => Keyboard.GetKeyStates(Key.LeftCtrl).HasFlag(KeyStates.Down)
        || Keyboard.GetKeyStates(Key.RightCtrl).HasFlag(KeyStates.Down);

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == FontSizeProperty
            || e.Property == FontFamilyProperty)
        {
            UpdateTextProperties(true);
            InvalidateVisual();
        }
        else if (e.Property == FontStyleProperty
            || e.Property == FontWeightProperty
            || e.Property == FontStretchProperty)
        {
            UpdateTextProperties();
            InvalidateVisual();
        }
    }

    private CommonTextProperties _textProperties;
    internal CodeCompletionTextSource? TextSource { get; private set; }

    private void UpdateTextProperties(bool updatesHeight = false)
    {
        if (updatesHeight) Height = Math.Ceiling(FontFamily.LineSpacing * FontSize); // 改行を想定してない
        _textProperties = new CommonTextProperties(FontSize, FontFamily, FontStyle, FontWeight, FontStretch);
        UpdateViewModel();
    }

    private void UpdateViewModel(object? oldValue, object? newValue)
    {
        void propChanged(object? sender, PropertyChangedEventArgs args)
        {
            var vm = (ViewModel)sender!;
            Show(vm);
        }

        if (oldValue is ViewModel oldVm) oldVm.PropertyChanged -= propChanged;
        if (newValue is ViewModel newVm) newVm.PropertyChanged += propChanged;

        UpdateViewModel(newValue);
    }

    private void UpdateViewModel(object? newValue)
    {
        TextSource = newValue is ViewModel vm && _textProperties is { } prop
            ? new(vm.Texts, prop, Height) // 改行を想定してない
            : null;
    }

    private void UpdateViewModel() => UpdateViewModel(DataContext);

    private static void ShowDiag(ViewModel vm)
    {
        var buffer = vm.Texts;
        var (t, p) = buffer.GetPosition();
        System.Diagnostics.Debug.WriteLine($"""
cursor: {buffer.Cursor} token: {t} pos: {p}
candidates: {string.Join(", ", vm.Candidates.Select(x => x.Text))} (selected: {vm.SelectedCandidateIndex})

""");
    }

    internal void UpdateCaret(double x)
    {
        _caret.Update(x, Height);
        _popup.IsOpen = true;
        _popup.HorizontalOffset = x;
    }
}
