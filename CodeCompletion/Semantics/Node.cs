namespace CodeCompletion.Semantics;

public abstract class Node
{
    public static Node Create(Type type)
    {
        return new PropertyNode(type);
    }

    public abstract IEnumerable<Candidate> GetCandidates();

    public void Filter(ReadOnlySpan<char> text, IList<Candidate> results)
    {
        results.Clear();
        foreach (var candidate in GetCandidates())
        {
            if (candidate.Text is not { } ct
                || ct.AsSpan().StartsWith(text, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(candidate);
            }
        }
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
