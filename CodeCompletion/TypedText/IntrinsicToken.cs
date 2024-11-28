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
}
