namespace CodeCompletion.Semantics;

public class KeywordNode(string keyword) : ValueNode
{
    public string Keyword { get; } = keyword;
    public override string ToString() => $"Keyword {Keyword}";
}

static class KeywordCandidate
{
    public static readonly FixedCandidate Null = new("null", new KeywordNode("null"));
    public static readonly FixedCandidate True = new("true", new KeywordNode("true"));
    public static readonly FixedCandidate False = new("false", new KeywordNode("false"));
}
