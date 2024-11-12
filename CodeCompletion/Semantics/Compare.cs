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

    private readonly Candidate[] _candidates =
        type == typeof(object) ? [KeywordCandidate.Null] :
        type == typeof(bool) ? [KeywordCandidate.True, KeywordCandidate.False] :
        //todo: nullable struct
        //todo: string は [ KeywordCandidate.Null, new LiteralCandidate(type)] にする？
        [new LiteralCandidate(type)];

    public override IEnumerable<Candidate> GetCandidates() => _candidates;

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

    private readonly CompareNode _singleton = new(type, comparison);

    public override Node GetNode() => _singleton;
}
