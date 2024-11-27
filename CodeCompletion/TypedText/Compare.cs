namespace CodeCompletion.TypedText;

public enum ComparisonType
{
    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual
}

public class CompareToken(Type type, ComparisonType comparison) : TypedToken
{
    public Type Type => type;
    public ComparisonType ComparisonType { get; } = comparison;

    private readonly Candidate[] _candidates =
        type == typeof(object) ? [KeywordCandidate.Null] :
        type == typeof(bool) ? [KeywordCandidate.True, KeywordCandidate.False] :
        //todo: nullable struct
        //todo: string は [ KeywordCandidate.Null, new LiteralCandidate(type)] にする？
        [new FixedCandidate(null, new LiteralToken(type))];

    public override IEnumerable<Candidate> GetCandidates(GetCandidatesContext context) => _candidates;

    public override string ToString() => $"Compare {type.Name} {ComparisonType}";
}

public class CompareCandidate(Type type, ComparisonType comparison) : Candidate
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

    private readonly CompareToken _singleton = new(type, comparison);

    public override TypedToken GetToken() => _singleton;
}
