namespace CodeCompletion.TypedText;

/// <summary>
/// <see cref="Text.Token"/> に型情報を持たせる。
/// </summary>
/// <remarks>
/// 型情報のみで、実際のトークン文字列は <see cref="Text.Token"/> を別途参照しないとダメ。
/// </remarks>
public abstract class TypedToken
{
    public static TypedToken Create(Type type)
    {
        return new PropertyToken(type);
    }

    /// <summary>
    /// 候補を取得。
    /// </summary>
    /// <param name="context">( の直前のプロパティ情報。</param>
    public abstract IEnumerable<Candidate> GetCandidates(PropertyHierarchy context);

    /// <summary>
    /// <see cref="GetCandidates"/> から、<paramref name="text"/> でフィルタリングした候補を返す。
    /// </summary>
    /// <param name="text">現在の打ちかけのトーケン文字列。</param>
    /// <param name="context">( の直前のプロパティ情報。</param>
    /// <param name="results">結果の格納先。</param>
    public void Filter(ReadOnlySpan<char> text, PropertyHierarchy context, IList<Candidate> results)
    {
        results.Clear();
        foreach (var candidate in GetCandidates(context))
        {
            if (candidate.Text is not { } ct
                || ct.AsSpan().StartsWith(text, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(candidate);
            }
        }
    }

    /// <summary>
    /// <see cref="Filter"/> の最初の1個の候補を返す。
    /// </summary>
    /// <param name="text">現在の打ちかけのトーケン文字列。</param>
    /// <param name="context">( の直前のプロパティ情報。</param>
    public virtual Candidate? Select(ReadOnlySpan<char> text, PropertyHierarchy context)
    {
        foreach (var candidate in GetCandidates(context))
        {
            if (candidate.Text is not { } ct
                || ct.AsSpan().StartsWith(text, StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }
        return null;

    }
}

/// <summary>
/// <see cref="TypedToken.GetCandidates"/> に <see cref="TypedTextModel"/> から渡す情報。
/// </summary>
/// <param name="Direct">
/// 直近の <see cref="PropertyTokenBase"/>。
/// リテラルのところで null, true, false とか出したりするのに使う。
/// </param>
/// <param name="Parenthesis">
/// ( 直前の <see cref="PropertyTokenBase"/>。
/// , | &amp; ) 後ろでの候補を出すために必要。
/// </param>
public record struct PropertyHierarchy(
    //PropertyTokenBase Direct, todo: 追加予定。
    PropertyTokenBase Parenthesis
    );
