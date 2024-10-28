using System.Collections.Immutable;

namespace CodeCompletion.Semantics;

enum ComparisonType
{
    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual
}

class CompareNode(ComparisonType comparison) : Node
{
    public override IEnumerable<Candidate> GetCandidates() => [];
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

    public override Node GetFactory() => new CompareNode(comparison);

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
