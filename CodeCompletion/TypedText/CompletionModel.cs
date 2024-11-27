using CodeCompletion.Text;

namespace CodeCompletion.TypedText;

public class CompletionModel(Type type)
{
    public TypedTextModel Semantics { get; } = new(type);

    public IReadOnlyList<Candidate> Candidates => _candidates;
    private readonly List<Candidate> _candidates = [];
    public int SelectedCandidateIndex { get; private set; }

    public TextBuffer Texts => Semantics.Texts;

    // (仮)
    //todo: 都度 Refresh を呼ぶんじゃなくて、1ストロークごとに更新処理掛ける。
    public void Refresh()
    {
        Semantics.Refresh();
        Semantics.GetCandidates(_candidates);
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

