using System.Collections.Immutable;

namespace CodeCompletion.Syntax;

enum ComparisonType
{
    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual
}

class CompareFactory(ComparisonType comparison) : Factory
{
    public override IEnumerable<Candidate> GetCandidates() => [];

    public override Candidate? Select(ReadOnlyMemory<char> text) => base.Select(text);
}

class CompareCandidate(ComparisonType comparison) : Candidate
{
    public override string Text => comparison switch
    {
        ComparisonType.Equal => "=",
        ComparisonType.NotEqual => "!=",
        ComparisonType.LessThan => "<",
        ComparisonType.LessThanOrEqual => "<=",
        ComparisonType.GreaterThan => ">",
        ComparisonType.GreaterThanOrEqual => ">=",
        _ => throw new NotImplementedException()
    };

    public override Factory GetFactory() => new CompareFactory(comparison);

    public static readonly ImmutableArray<Candidate> Singleton =
    [
        new CompareCandidate(ComparisonType.Equal),
        new CompareCandidate(ComparisonType.NotEqual),
        new CompareCandidate(ComparisonType.LessThan),
        new CompareCandidate(ComparisonType.LessThanOrEqual),
        new CompareCandidate(ComparisonType.GreaterThan),
        new CompareCandidate(ComparisonType.GreaterThanOrEqual)
    ];
}
