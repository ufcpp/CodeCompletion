namespace ObjectMatching;

public enum ComparisonType
{
    Equal = 1,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,
    Tilde, // ~ は正規表現だけじゃなく、enum.HasFlag マッチにも使うことに。
}
