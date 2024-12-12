using CodeCompletion.Text;
using System.Globalization;
using System.Runtime.InteropServices;

namespace CodeCompletion.Completion;

public class CompletionModel(ICompletionContext context)
{
    public string? Description { get; private set; }
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
        GetCandidates();
    }

    private readonly List<Candidate> _temp = [];

    private void GetCandidates()
    {
        var (pos, _) = Texts.GetPosition();
        var previousToken = pos == 0 ? "" : Texts.Tokens[pos - 1].Span;
        var text = Texts.Tokens[pos].Span;

        (Description, var candidates) = Context.GetCandidates(previousToken, pos);
        FilterCandidates(text, candidates);
    }

    private void FilterCandidates(ReadOnlySpan<char> text, IEnumerable<Candidate> candidates)
    {
        _temp.Clear();
        _temp.AddRange(candidates);
        var temp = CollectionsMarshal.AsSpan(_temp);

        var results = _candidates;
        results.Clear();

        foreach (var pred in _predicates)
        {
            if (!FilterCandidates(false, text, ref temp, results, pred)) break;
        }

        if (temp.Length == 0) return;

        // Text で見つからなかったら、Description でも探す
        foreach (var pred in _predicates)
        {
            if (!FilterCandidates(true, text, ref temp, results, pred)) break;
        }

        static bool FilterCandidates(bool useDescritption, ReadOnlySpan<char> text, ref Span<Candidate> candidates, List<Candidate> results, Func<ReadOnlySpan<char>, ReadOnlySpan<char>, bool> match)
        {
            var count = 0;
            for (int i = 0; i < candidates.Length; i++)
            {
                var c = candidates[i];
                var candidateText = useDescritption ? c.Description : c.Text;

                // results.Add(c); candidates.Remove(c); 相当処理。
                if (candidateText != null && match(candidateText, text))
                {
                    results.Add(c);
                }
                else
                {
                    if (i != count) candidates[count] = c;
                    count++;
                }
            }
            candidates = candidates[..count];
            return candidates.Length > 0;
        }
    }

    private static readonly Func<ReadOnlySpan<char>, ReadOnlySpan<char>, bool>[] _predicates =
    [
        // 前に来てほしい検索条件から順に:

        // 先頭、case 一致
        (candidate, text) => candidate.StartsWith(text, StringComparison.Ordinal),
        // 先頭、case 不問
        (candidate, text) => candidate.StartsWith(text, StringComparison.OrdinalIgnoreCase),
        // どこでも、case 一致
        (candidate, text) => candidate.Contains(text, StringComparison.Ordinal),
        // どこでも、case 不問
        (candidate, text) => candidate.Contains(text, StringComparison.OrdinalIgnoreCase),
        // CamelCase の各単語の頭文字で検索 (例えば AbcDefGhi なら adg に一致)
        CamelCaseMatch,
    ];

    private static bool CamelCaseMatch(ReadOnlySpan<char> candidate, ReadOnlySpan<char> text)
    {
        var temp = (stackalloc char[candidate.Length]);
        var i = 0;

        var prev = (UnicodeCategory)(-1);
        foreach (var c in candidate)
        {
            var cat = char.GetUnicodeCategory(c);
            if (cat == UnicodeCategory.UppercaseLetter // 大文字は単語の先頭扱い
                || (prev != cat && prev != UnicodeCategory.UppercaseLetter) // abcあいうdef みたいなとき、 a, あ, d を先頭扱いしたい
                ) temp[i++] = c;
            prev = cat;
            if (i == temp.Length) break;
        }

        ReadOnlySpan<char> initials = temp[..i];
        return initials.Contains(text, StringComparison.OrdinalIgnoreCase);
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

