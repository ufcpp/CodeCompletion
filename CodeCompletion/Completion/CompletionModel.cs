using CodeCompletion.Text;

namespace CodeCompletion.Completion;

public class CompletionModel(ICompletionContext context)
{
    public IReadOnlyList<Candidate> Candidates => _candidates;
    private readonly List<Candidate> _candidates = [];
    public int SelectedCandidateIndex { get; private set; }

    public ICompletionContext Context { get; } = context;
    public TextBuffer Texts { get; } = new();

    public void Reset(ReadOnlySpan<char> source)
    {
        Texts.Reset(source);
        Refresh();
    }

    // (仮)
    //todo: 都度 Refresh を呼ぶんじゃなくて、1ストロークごとに更新処理掛ける。
    public void Refresh()
    {
        Context.Refresh(Texts);
        GetCandidates(_candidates);
    }

    public void GetCandidates(IList<Candidate> results)
    {
        results.Clear();
        var (pos, _) = Texts.GetPosition();
        var previousToken = pos == 0 ? "" : Texts.Tokens[pos - 1].Span;
        var text = Texts.Tokens[pos].Span;

        foreach (var candidate in Context.GetCandidates(previousToken, pos))
        {
            if (candidate.Text is not { } ct
                || ct.AsSpan().StartsWith(text, StringComparison.OrdinalIgnoreCase)) //todo: ここのマッチ方法もインターフェイスで変更可能にしたい。
            {
                results.Add(candidate);
            }
        }
    }

    /// <summary>
    /// 補完候補確定。
    /// </summary>
    public bool Complete()
    {
        if (Candidates.ElementAtOrDefault(SelectedCandidateIndex) is { Text: { } ct })
        {
            Texts.Replace(ct);
            SelectedCandidateIndex = 0;
            return true;
        }

        return false;
    }

    // 補完候補を1個次に。
    public void Next()
    {
        SelectedCandidateIndex++;
        if (SelectedCandidateIndex >= Candidates.Count) SelectedCandidateIndex = 0;
    }

    // 補完候補を1個前に。
    public void Prev()
    {
        SelectedCandidateIndex--;
        if (SelectedCandidateIndex < 0) SelectedCandidateIndex = Candidates.Count - 1;
    }
}

