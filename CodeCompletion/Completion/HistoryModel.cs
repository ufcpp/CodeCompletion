using CodeCompletion.Text;

namespace CodeCompletion.Completion;

public class HistoryModel(CompletionModel completion, int? maxHisotry = null)
{
    public CompletionModel Completion { get; } = completion;

    private const int DefaultMaxHistory = 10;

    public int MaxHistory { get; } = maxHisotry ?? DefaultMaxHistory;

    public HistoryModel(ICompletionContext context, int? maxHisotry = null) : this(new CompletionModel(context), maxHisotry) { }

    public int Index { get; private set; }

    public TextBuffer Texts => Completion.Texts;

    private readonly List<string> _history = new();

    public void Save()
    {
        if (Texts.TotalLength == 0) return;
        var text = Texts.ToString();
        if (_history.Contains(text)) return;
        _history.Insert(0, text);
        if (_history.Count > MaxHistory) _history.RemoveAt(_history.Count - 1);
        Index = 0;
    }

    public void Reset(ReadOnlySpan<char> source)
    {
        foreach (var lineRange in source.Split('\n'))
        {
            var line = source[lineRange];
            if (line.IsWhiteSpace()) continue;
            Completion.Reset(line);
            Save();
        }
    }

    private void ChooseHisotry()
    {
        var i = Index;
        if (i < _history.Count) Texts.Reset(_history[i]);
    }

    // 補完候補を1個次に。
    public void Next()
    {
        Index++;
        if (Index >= _history.Count) Index = 0;
        ChooseHisotry();
    }

    // 補完候補を1個前に。
    public void Prev()
    {
        Index--;
        if (Index < 0) Index = _history.Count - 1;
        ChooseHisotry();
    }

    // Copy → Paste で元に戻るようにするには逆順で Join。
    public override string ToString() => string.Join("\n", Enumerable.Reverse(_history));
}

