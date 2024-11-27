namespace CodeCompletion.Syntax;

public enum NodeType : byte
{
    Error,
    Member,
    Comma,
    Or,
    And,

    Value,

    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,

    Regex,
}
