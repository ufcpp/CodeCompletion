namespace ObjectFilter.Syntax;

public enum NodeType : byte
{
    /// <summary>
    /// 不正な式を Parse した時だけ出るはず。
    /// </summary>
    Null,

    /// <summary>
    /// プロパティ参照とか組み込みメンバー( .length とか)。
    /// </summary>
    Member,

    /// <summary>
    /// ,
    /// </summary>
    Comma,

    /// <summary>
    /// |
    /// </summary>
    Or,

    /// <summary>
    /// &amp;
    /// </summary>
    And,

    // 当初 Value を用意してたけど、比較系ノード(Equal とか)に統合
    // (比較演算子の直後にしか来ないので、Equal とかがトークン2つ参照)。
    //Value,

    // & 0b111 で ComparisonType に変換できるようにする。
    Equal = 8 + ComparisonType.Equal,
    NotEqual = 8 + ComparisonType.NotEqual,
    LessThan = 8 + ComparisonType.LessThan,
    LessThanOrEqual = 8 + ComparisonType.LessThanOrEqual,
    GreaterThan = 8 + ComparisonType.GreaterThan,
    GreaterThanOrEqual = 8 + ComparisonType.GreaterThanOrEqual,
    Regex = 8 + ComparisonType.Tilde,
}

internal static class NodeTypeExtensions
{
    public static bool IsComparison(this NodeType type) => (byte)type > 0b111;
    public static ComparisonType ToComparisonType(this NodeType type) => (ComparisonType)((byte)type & 0b111);
}
