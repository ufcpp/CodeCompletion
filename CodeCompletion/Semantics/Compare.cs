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
    public override IEnumerable<Candidate> GetCandidates() => [
        //todo: LiteralNode
        ];
}

class CompareCandidate(PrimitiveCategory category, ComparisonType comparison) : Candidate
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

    public override Node GetFactory() => new CompareNode(category, comparison);
}
