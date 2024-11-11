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

class CompareNode(PrimitiveCategory category, ComparisonType comparison) : Node
{
    public ComparisonType ComparisonType { get; } = comparison;

    public override IEnumerable<Candidate> GetCandidates() => [
            new LiteralCandidate(category),
        ];

    public override string ToString() => $"Compare {category} {ComparisonType}";
}

class CompareCandidate(PrimitiveCategory category, ComparisonType comparison) : Candidate
{
    public override string? Text => comparison switch
    {
        ComparisonType.Equal => "=",
        ComparisonType.NotEqual => "!=",
        ComparisonType.LessThan => "<",
        ComparisonType.LessThanOrEqual => "<=",
        ComparisonType.GreaterThan => ">",
        ComparisonType.GreaterThanOrEqual => ">=",
        _ => throw new NotImplementedException()
    };

    public override Node GetNode() => new CompareNode(category, comparison);
}
