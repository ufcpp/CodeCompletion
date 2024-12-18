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

        Loaded += (_, _) =>
        {
            UpdateTextProperties(true);
            if (DataContext is ViewModel vm) Reflesh(vm);
        };
        DataContextChanged += (_, args) => UpdateViewModel(args.OldValue, args.NewValue);

        //todo: InputBindings KeyBinding でやった方がいい？
        TextInput += (sender, e) =>
        {
            if (DataContext is not ViewModel vm) return;

            if (e.Text.Length == 0) return;
            if (char.GetUnicodeCategory(e.Text[0]) == System.Globalization.UnicodeCategory.Control) return;

            vm.Texts.Insert(e.Text);
            Reflesh(vm);
            e.Handled = true;
        };

        KeyDown += (sender, e) =>
        {
            if (DataContext is not ViewModel vm) return;

            var (handled, invalidates) = Keybind.Handle(e.Key, CtrlKeyDown, vm);
            if (invalidates) Reflesh(vm);
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

    private void Reflesh(ViewModel vm)
    {
        vm.Refresh();
        Invalidate();
    }

    private void Invalidate()
    {
        _text.InvalidateVisual();
        InvalidateVisual();
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
            Invalidate();
        }
        else if (e.Property == FontStyleProperty
            || e.Property == FontWeightProperty
            || e.Property == FontStretchProperty)
        {
            UpdateTextProperties();
            Invalidate();
        }
    }

    private CommonTextProperties _textProperties;
    internal CodeCompletionTextSource? TextSource { get; private set; }

    private void UpdateTextProperties(bool updatesHeight = false)
    {
        if (updatesHeight) Height = Math.Ceiling(FontFamily.LineSpacing * FontSize) + 2 * (MarginSize + BorderSize); // 改行を想定してない
        _textProperties = new CommonTextProperties(FontSize, FontFamily, FontStyle, FontWeight, FontStretch);

        if (DataContext is ViewModel vm) UpdateViewModel(vm, _textProperties);
    }

    private void UpdateViewModel(object? oldValue, object? newValue)
    {
        void propChanged(object? sender, PropertyChangedEventArgs args)
        {
            var vm = (ViewModel)sender!;
            Invalidate();
        }

        if (oldValue is ViewModel oldVm) oldVm.PropertyChanged -= propChanged;

        if (newValue is ViewModel newVm)
        {
            newVm.PropertyChanged += propChanged;

            if (_textProperties is { } prop) UpdateViewModel(newVm, prop);
        }
    }

    private void UpdateViewModel(ViewModel vm, CommonTextProperties prop)
    {
        TextSource = new(vm.Texts, prop, CanvasHeight); // 改行を想定してない
        if (IsLoaded) Reflesh(vm);
    }

    internal void UpdateCaret(double x)
    {
        if(!IsLoaded) return;
        _caret.Update(x, CanvasHeight);
        _popup.HorizontalOffset = x;
        SetVisible(IsFocused);
    }
}
