using CodeCompletion.Completion;
using CodeCompletion.Text;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CodeCompletion.ViewModels;

public class ViewModel<TCompletionContext>(TCompletionContext context, IEnumerable itemsSource) : ViewModel(context, itemsSource)
    where TCompletionContext : ICompletionContext
{
    new protected TCompletionContext Context => (TCompletionContext)base.Context;
}

public class ViewModel : INotifyPropertyChanged
{
    public IEnumerable ItemsSource { get; }
    protected ICompletionContext Context { get; }
    public CompletionModel Completion { get; }

    public ViewModel(ICompletionContext context, IEnumerable itemsSource)
    {
        ItemsSource = itemsSource;
        Context = context;
        Completion = new(Context);
        _filteredItems = itemsSource;
    }

    public TextBuffer Texts => Completion.Texts;

    public virtual void Refresh()
    {
        Completion.Refresh();
        Description = Completion.Description;
        _candidates?.Invalidate();
        SelectedCandidateIndex = Completion.SelectedCandidateIndex;
        Refreshed?.Invoke(this);
    }

    public event Action<ViewModel>? Refreshed;

    private string? _description;
    public string? Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                PropertyChanged?.Invoke(this, DescriptionChanged);
            }
        }
    }
    private static readonly PropertyChangedEventArgs DescriptionChanged = new(nameof(Description));

    private Wrap<Candidate>? _candidates;
    public IEnumerable<Candidate> Candidates => _candidates ??= new(Completion.Candidates);

    private int _index;
    public int SelectedCandidateIndex
    {
        get => _index;
        set
        {
            if (_index != value)
            {
                _index = value;
                PropertyChanged?.Invoke(this, SelectedCandidateIndexChanged);
            }
        }
    }
    private static readonly PropertyChangedEventArgs SelectedCandidateIndexChanged = new(nameof(SelectedCandidateIndex));

    public bool Complete() => Completion.Complete();
    public void Next() => Completion.Next();
    public void Prev() => Completion.Prev();

    private IEnumerable _filteredItems;
    public IEnumerable FilteredItems
    {
        get => _filteredItems;
        private set { _filteredItems = value; PropertyChanged?.Invoke(this, FilteredItemsChanged); }
    }
    private static readonly PropertyChangedEventArgs FilteredItemsChanged = new(nameof(FilteredItems));

    public void Filter()
    {
        FilteredItems = Filter(ItemsSource);
    }

    protected virtual IEnumerable Filter(IEnumerable itemsSource) => itemsSource;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Reset(ReadOnlySpan<char> source)
    {
        Completion.Reset(source);
        PropertyChanged?.Invoke(this, TextsChanged);
    }

    private static readonly PropertyChangedEventArgs TextsChanged = new(nameof(Texts));
}

class Wrap<T>(IEnumerable<T> inner) : IEnumerable<T>, INotifyCollectionChanged
{
    public IEnumerator<T> GetEnumerator() => inner.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)inner).GetEnumerator();

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    private static readonly NotifyCollectionChangedEventArgs _reset = new(NotifyCollectionChangedAction.Reset);

    public void Invalidate() => CollectionChanged?.Invoke(this, _reset);
}

