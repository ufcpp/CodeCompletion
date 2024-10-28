namespace CodeCompletion.Semantics;

public enum PrimitiveCategory
{
    Integer,
    Float,
    String,

    //todo: Date 系
}

public class PrimitivePropertyNode(PrimitiveCategory category) : Node
{
    public override IEnumerable<Candidate> GetCandidates() => _candidates;

    private readonly Candidate[] _candidates =
    [
        new CompareCandidate(category, ComparisonType.Equal),
        new CompareCandidate(category, ComparisonType.NotEqual),
        new CompareCandidate(category, ComparisonType.LessThan),
        new CompareCandidate(category, ComparisonType.LessThanOrEqual),
        new CompareCandidate(category, ComparisonType.GreaterThan),
        new CompareCandidate(category, ComparisonType.GreaterThanOrEqual)
    ];

    public static readonly PrimitivePropertyNode Integer = new(PrimitiveCategory.Integer);
    public static readonly PrimitivePropertyNode Float = new(PrimitiveCategory.Float);
    public static readonly PrimitivePropertyNode String = new(PrimitiveCategory.String);
}
