namespace CodeCompletion.Semantics;

public class KeywordNode(string keyword) : Node
{
    public string Keyword { get; } = keyword;

    public override IEnumerable<Candidate> GetCandidates(GetCandidatesContext context) => [
            // , &, |, )
        ];
    public override string ToString() => $"Keyword {Keyword}";
}

public class KeywordCandidate(string keyword) : Candidate
{
    public override string? Text => keyword;

    private readonly KeywordNode _singleton = new(keyword);

    public override Node GetNode() => _singleton;

    public static readonly KeywordCandidate Null = new("null");
    public static readonly KeywordCandidate True = new("true");
    public static readonly KeywordCandidate False = new("false");
}
