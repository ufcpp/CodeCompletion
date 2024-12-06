using CodeCompletion.Completion;
using CodeCompletion.ViewModels;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CodeCompletion.Controls;

public partial class CandidateSelector : UserControl
{
    public CandidateSelector()
    {
        InitializeComponent();

        DataContextChanged += static (sender, arg) =>
        {
            var @this = (CandidateSelector)sender;

            if (arg.OldValue is ViewModel x)
            {
                x.PropertyChanged -= @this.VMPropertyChanged;
                ((INotifyCollectionChanged)x.Candidates).CollectionChanged -= @this.VMCPropertyChanged;
            }

            if (arg.NewValue is ViewModel y)
            {
                y.PropertyChanged += @this.VMPropertyChanged;
                ((INotifyCollectionChanged)y.Candidates).CollectionChanged += @this.VMCPropertyChanged;
            }
        };
    }

    private void VMCPropertyChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (DataContext is not ViewModel vm) return;
        OnPropertyChanged(vm, true);
    }

    private void VMPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ViewModel vm) return;

        var n = e.PropertyName;
        if (n == nameof(ViewModel.SelectedCandidateIndex))
        {
            OnPropertyChanged(vm, true);
        }
        else if (n == nameof(ViewModel.Description))
        {
            OnPropertyChanged(vm, false);
        }
    }

    private void OnPropertyChanged(ViewModel vm, bool needsScroll)
    {
        if (needsScroll) Scroll(vm);

        var listVisible = vm.Candidates.Any();
        list.Visibility = vis(listVisible);

        var descVisible = !string.IsNullOrWhiteSpace(vm.Description);
        desc.Visibility = vis(descVisible);

        Visibility = vis(listVisible || descVisible);

        static Visibility vis(bool x) => x ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Scroll(ViewModel vm)
    {
        if (vm.Candidates?.ElementAtOrDefault(vm.SelectedCandidateIndex) is not { } x) return;
        list.ScrollIntoView(x);
    }
}
