using CodeCompletion.Semantics;
using CodeCompletion.Text;
using System.Collections;
using System.ComponentModel;

namespace TrialWpfApp;

public class ViewModel(IEnumerable itemsSource) : INotifyPropertyChanged
{
    public IEnumerable ItemsSource { get; } = itemsSource;

    private static Type GetElementType(object obj)
    {
        var t = obj.GetType();
        if (t.IsArray) return t.GetElementType()!;

        foreach (var i in t.GetInterfaces())
        {
            if (!i.IsGenericType) continue;

            var gt = t.GetGenericTypeDefinition();

            if (gt == typeof(IEnumerable<>)) return i.GetGenericArguments()[0];
        }

        throw new InvalidOperationException();
    }

    public CompletionModel Completion { get; set; } = new(GetElementType(itemsSource));
    public SemanticModel Semantics => Completion.Semantics;
    public TextBuffer Texts => Semantics.Texts;

    public void Refresh() => Completion.Refresh();
    public IReadOnlyList<Candidate> Candidates => Completion.Candidates;
    public int SelectedCandidateIndex => Completion.SelectedCandidateIndex;
    public bool Complete() => Completion.Complete();
    public void Next() => Completion.Next();
    public void Prev() => Completion.Prev();

    private IEnumerable _filteredItems = itemsSource;
    public IEnumerable FilteredItems
    {
        get => _filteredItems;
        private set { _filteredItems = value; PropertyChanged?.Invoke(this, _filteredItemsChanged); }
    }
    private static readonly PropertyChangedEventArgs _filteredItemsChanged = new(nameof(FilteredItems));

    public void Filter()
    {
        var filter = Semantics.Emit();

        if (filter is null)
        {
            System.Diagnostics.Debug.WriteLine("filter OFF");
            FilteredItems = ItemsSource;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("filter ON");
            FilteredItems = ItemsSource.Cast<object>().Where(filter).ToList();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

