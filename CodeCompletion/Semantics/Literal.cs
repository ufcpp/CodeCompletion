namespace CodeCompletion.Semantics;

internal class LiteralNode(PrimitiveCategory category) : Node
{
    public override IEnumerable<Candidate> GetCandidates() => [
            // , &, |, )
        ];

    public override string ToString() => $"Literal {category}";
}

class LiteralCandidate(PrimitiveCategory category) : Candidate
{
    public override string? Text => null;

    public override Node GetNode() => new LiteralNode(category);
}