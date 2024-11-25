namespace CodeCompletion.Semantics;

public class LiteralNode(Type type) : Node
{
    public override IEnumerable<Candidate> GetCandidates(GetCandidatesContext context) => [
            // , &, |, )
        ];

    public override string ToString() => $"Literal {type.Name}";
}

public class LiteralCandidate(Type type) : Candidate
{
    public override string? Text => null;

    private readonly LiteralNode _singleton = new(type);

    public override Node GetNode() => _singleton;
}
