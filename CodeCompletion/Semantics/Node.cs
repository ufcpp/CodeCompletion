namespace CodeCompletion.Semantics;

public abstract class Node
{
    public static Node Create(Type type)
    {
        return new PropertyNode(type);
    }

    public abstract IEnumerable<Candidate> GetCandidates();

    public IReadOnlyList<Candidate> Filter(ReadOnlySpan<char> text)
    {
        List<Candidate>? candidates = null;
        foreach (var candidate in GetCandidates())
        {
            if (candidate.Text is not { } ct
                || ct.AsSpan().StartsWith(text, StringComparison.OrdinalIgnoreCase))
            {
                (candidates ??= []).Add(candidate);
            }
        }
        return candidates ?? [];
    }

    public virtual Candidate? Select(ReadOnlySpan<char> text)
    {
        foreach (var candidate in GetCandidates())
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
