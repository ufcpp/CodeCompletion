
namespace CodeCompletion.TypedText;

/// <summary>
/// 文字列に対する .length みたいな、組み込みプロパティ。
/// </summary>
public class IntrinsicToken(string name, Type sourceType, Type resultType) : PrimitivePropertyToken(resultType)
{
    public string Name { get; } = name;
    public Type SourceType { get; } = sourceType;
    public override string ToString() => $"Intrinsic {Name}";
}

internal class IntrinsicNames
{
    public const string Length = ".length";
    public const string Ceiling = ".ceil";
    public const string Floor = ".floor";
    public const string Round = ".round";
    public const string Any = ".any";
    public const string All = ".all";
}

public class ArrayToken(PropertyToken parent) : PropertyTokenBase
{
    //todo: 今、元プロパティの候補を全部素通しだけど、 .any .all は除外した方がいいかも。
    public override IEnumerable<Candidate> GetCandidates(PropertyTokenBase context) => parent.GetCandidates(context);
}

public class ArrayAnyToken(PropertyToken parent) : ArrayToken(parent);
public class ArrayAllToken(PropertyToken parent) : ArrayToken(parent);
