namespace CodeCompletion.Semantics;

public abstract class Node
{
    public static Node Create(Type type)
    {
        return new PropertyNode(type);
    }

    public abstract IEnumerable<Candidate> GetCandidates(GetCandidatesContext context);

    public void Filter(ReadOnlySpan<char> text, GetCandidatesContext context, IList<Candidate> results)
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

    public virtual Candidate? Select(ReadOnlySpan<char> text, GetCandidatesContext context)
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
/// <see cref="Node.GetCandidates"/> に渡すコンテキスト。
/// </summary>
/// <remarks>
/// A B (C1 > 1, C2 < 5) みたいなのを作れるようにする予定。
/// , の後ろで ( の直前の Node から取れる候補に巻き戻さないとダメで、そのために ( のたびにその直前を FILO 保存する必要あり。
///
/// 現状は ( を実装してないので、常に root ノードだけ保持。
/// </remarks>
public readonly struct GetCandidatesContext(Node root)
{
    public Node Root { get; } = root;
}
