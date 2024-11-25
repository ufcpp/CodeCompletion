namespace CodeCompletion.Semantics;

public class PrimitivePropertyNode(Type type) : Node
{
    private static readonly Type[] _comparableTypes =
    [
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(short),
        typeof(ushort),
        typeof(byte),
        typeof(sbyte),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(TimeSpan),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(DateOnly),
        typeof(TimeOnly),
        typeof(bool),
        typeof(string),
    ];

    private static readonly Dictionary<Type, PrimitivePropertyNode> _map = _comparableTypes.ToDictionary(t => t, t => new PrimitivePropertyNode(t));

    public static PrimitivePropertyNode? Get(Type type) => _map.TryGetValue(type, out var x) ? x : null;

    public Type Type { get; } = type;

    public override IEnumerable<Candidate> GetCandidates(GetCandidatesContext context) => _candidates;

    private readonly Candidate[] _candidates =
    [
        new CompareCandidate(type, ComparisonType.Equal),
        new CompareCandidate(type, ComparisonType.NotEqual),
        new CompareCandidate(type, ComparisonType.LessThan),
        new CompareCandidate(type, ComparisonType.LessThanOrEqual),
        new CompareCandidate(type, ComparisonType.GreaterThan),
        new CompareCandidate(type, ComparisonType.GreaterThanOrEqual)

        //todo: String のとき、 Length

        //todo: Float のとき、 Ceiling, Floor, Round

        //todo: 時刻系、hour, min, sec, ...?

        //todo: nullable primitive のとき new CompareCandidate(typeof(object), ComparisonType.Equal) 足すので null が候補に出る？
    ];

    public override string ToString() => $"Property of {Type.Name}";
}
