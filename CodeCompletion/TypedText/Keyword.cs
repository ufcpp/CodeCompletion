namespace CodeCompletion.TypedText;

public class KeywordToken(string keyword) : ValueToken
{
    public string Keyword { get; } = keyword;
    public override string ToString() => $"Keyword {Keyword}";
}

static class KeywordCandidate
{
    public static readonly FixedCandidate Null = new("null", new KeywordToken("null"));
    public static readonly FixedCandidate True = new("true", new KeywordToken("true"));
    public static readonly FixedCandidate False = new("false", new KeywordToken("false"));
}
