namespace CodeCompletion.Semantics;

public class PrimitivePropertyNode(Type type) : Node
{
    private static readonly PrimitivePropertyNode _int = new(typeof(int));

    private static readonly Dictionary<Type, PrimitivePropertyNode> _map = new()
    {
        { typeof(int), _int },
        { typeof(uint), new(typeof(uint)) },
        { typeof(long), new(typeof(long)) },
        { typeof(ulong), new(typeof(ulong)) },
        { typeof(short), new(typeof(short)) },
        { typeof(ushort), new(typeof(ushort)) },
        { typeof(byte), new(typeof(byte)) },
        { typeof(sbyte), new(typeof(sbyte)) },
        { typeof(float), new(typeof(float)) },
        { typeof(double), new(typeof(double)) },
        { typeof(decimal), new(typeof(decimal)) },
        { typeof(TimeSpan), new(typeof(TimeSpan)) },
        { typeof(DateTime), new(typeof(DateTime)) },
        { typeof(DateTimeOffset), new(typeof(DateTimeOffset)) },
        { typeof(DateOnly), new(typeof(DateOnly)) },
        { typeof(TimeOnly), new(typeof(TimeOnly)) },
        { typeof(bool), new(typeof(bool)) },
        { typeof(string), new(typeof(string)) },
    };

    public static PrimitivePropertyNode? Get(Type type) => _map.TryGetValue(type, out var x) ? x : null;

    public Type Type { get; } = type;

    public override IEnumerable<Candidate> GetCandidates(GetCandidatesContext context) => _candidates;

    private readonly Candidate[] _candidates = CreateCandidates(type);

    private static Candidate[] CreateCandidates(Type type)
    {
        if (type == typeof(string)) return
        [
            new CompareCandidate(type, ComparisonType.Equal),
            new CompareCandidate(type, ComparisonType.NotEqual),
            new CompareCandidate(type, ComparisonType.LessThan),
            new CompareCandidate(type, ComparisonType.LessThanOrEqual),
            new CompareCandidate(type, ComparisonType.GreaterThan),
            new CompareCandidate(type, ComparisonType.GreaterThanOrEqual),
            new FixedCandidate(IntrinsicNames.Length, new IntrinsicNode(IntrinsicNames.Length, typeof(string), typeof(int))),
        ];
        else if (type == typeof(float) || type == typeof(double) || type == typeof(decimal)) return
        [
            new CompareCandidate(type, ComparisonType.Equal),
            new CompareCandidate(type, ComparisonType.NotEqual),
            new CompareCandidate(type, ComparisonType.LessThan),
            new CompareCandidate(type, ComparisonType.LessThanOrEqual),
            new CompareCandidate(type, ComparisonType.GreaterThan),
            new CompareCandidate(type, ComparisonType.GreaterThanOrEqual),
            new FixedCandidate(IntrinsicNames.Ceiling, new IntrinsicNode(IntrinsicNames.Ceiling, type, typeof(long))),
            new FixedCandidate(IntrinsicNames.Floor, new IntrinsicNode(IntrinsicNames.Floor, type, typeof(long))),
            new FixedCandidate(IntrinsicNames.Round, new IntrinsicNode(IntrinsicNames.Round, type, typeof(long))),
        ];
        else return
        [
            new CompareCandidate(type, ComparisonType.Equal),
            new CompareCandidate(type, ComparisonType.NotEqual),
            new CompareCandidate(type, ComparisonType.LessThan),
            new CompareCandidate(type, ComparisonType.LessThanOrEqual),
            new CompareCandidate(type, ComparisonType.GreaterThan),
            new CompareCandidate(type, ComparisonType.GreaterThanOrEqual),
        ];

        //todo: Float のとき、 Ceiling, Floor, Round
        //todo: 時刻系、hour, min, sec, ...?
    }

    public override string ToString() => $"Property of {Type.Name}";
}
