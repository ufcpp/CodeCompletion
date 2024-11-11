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

class CompareNode(Type type, ComparisonType comparison) : Node
{
    public ComparisonType ComparisonType { get; } = comparison;

    public override IEnumerable<Candidate> GetCandidates() => [
            new LiteralCandidate(type),
        ];

    public override string ToString() => $"Compare {type.Name} {ComparisonType}";
}

class CompareCandidate(Type type, ComparisonType comparison) : Candidate
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

    public override Node GetNode() => new CompareNode(type, comparison);
}
