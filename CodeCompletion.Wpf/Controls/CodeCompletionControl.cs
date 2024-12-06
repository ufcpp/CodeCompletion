using CodeCompletion.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CodeCompletion.Controls;

public partial class CodeCompletionControl : ContentControl
{
    private readonly TextView _text;
    private readonly CaretView _caret;
    private readonly CandidateSelector _candidates;
    private readonly Popup _popup;

    const int MarginSize = 3;
    const int BorderSize = 1;

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

        var canvas = new Canvas
        {
            Children = { _text, _caret.Line, _popup },
            Background = Brushes.White,
            Margin = new(MarginSize),
        };
        canvas.MouseDown += (s, e) => Focus();

        this.BindCopyAndPaste();

        Content = new Border
        {
            Child = canvas,
            BorderThickness = new(BorderSize),
            BorderBrush = Brushes.DarkGray,
        };

        Focusable = true;

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

        GotFocus += (_, _) => SetVisible(true);
        LostFocus += (_, _) => SetVisible(false);
    }

    private double CanvasHeight => ActualHeight - MarginSize * 2 - BorderSize * 2;

    private void SetVisible(bool isVisible)
    {
        _popup.IsOpen = isVisible;
        _caret.Line.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Show(ViewModel vm)
    {
        vm.Refresh();
        _text.InvalidateVisual();
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
        if (updatesHeight) Height = Math.Ceiling(FontFamily.LineSpacing * FontSize) + 2 * (MarginSize + BorderSize); // 改行を想定してない
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
        if (newValue is not ViewModel vm) return;

        TextSource = _textProperties is { } prop
            ? new(vm.Texts, prop, CanvasHeight) // 改行を想定してない
            : null;

        Show(vm);
    }

    private void UpdateViewModel() => UpdateViewModel(DataContext);

    internal void UpdateCaret(double x)
    {
        _caret.Update(x, CanvasHeight);
        _popup.HorizontalOffset = x;
        SetVisible(IsFocused);
    }
}
