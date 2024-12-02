using CodeCompletion.Completion;
using CodeCompletion.Text;
using ObjectMatching.Completion;
using ObjectMatching.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace TrialWpfApp;

public class ViewModel : INotifyPropertyChanged
{
    public IEnumerable ItemsSource { get; }
    private readonly CompletionContext _context;
    public CompletionModel Completion { get; }

    public ViewModel(IEnumerable itemsSource, ITypeProvider? typeProvider = null)
    {
        ItemsSource = itemsSource;
        _context = new(new(GetElementType(itemsSource), typeProvider ?? new DefaultTypeProvider()), new());
        Completion = new(_context);
        _filteredItems = itemsSource;
    }

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

    public TextBuffer Texts => Completion.Texts;

    public void Refresh()
    {
        Completion.Refresh();
        _candidates?.Invalidate();
        SelectedCandidateIndex = Completion.SelectedCandidateIndex;
    }

    private Wrap<Candidate>? _candidates;
    public IEnumerable<Candidate> Candidates => _candidates ??= new(Completion.Candidates.Where(x => x.Text != null));

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
        var filter = _context.Emit();

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

    public void Reset(ReadOnlySpan<char> source)
    {
        _context.Reset(source);
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

static class WrapExtensions
{
    public static Wrap<T> Wrap<T>(this IReadOnlyList<T> inner) => new(inner);
}

