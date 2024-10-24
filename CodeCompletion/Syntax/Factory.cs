using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCompletion.Syntax;

public abstract class Factory
{
    public static Factory Create(Type type)
    {
        return new PropertyFactory(type);
    }

    public abstract IEnumerable<Candidate> GetCandidates();

    public IEnumerable<Candidate> Filter(ReadOnlyMemory<char> text)
    {
        foreach (var candidate in GetCandidates())
        {
            if (candidate.Text.AsSpan().StartsWith(text.Span, StringComparison.OrdinalIgnoreCase))
            {
                yield return candidate;
            }
        }
    }

    public virtual Candidate? Select(ReadOnlyMemory<char> text)
        => Filter(text).FirstOrDefault();
}
