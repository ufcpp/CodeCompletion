using CodeCompletion.ViewModels;
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

            if (arg.OldValue is ViewModel x) x.Refreshed -= @this.OnRefleshed;
            if (arg.NewValue is ViewModel y) y.Refreshed += @this.OnRefleshed;
        };
    }

    private void OnRefleshed(ViewModel vm)
    {
        Scroll(vm);

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
