namespace CodeCompletion.Semantics;

public abstract class Candidate
{
    public abstract string? Text { get; }
    public abstract Node GetNode();
}

/// <summary>
/// 常に固定の <see cref="Text"/>, <see cref="Node"/> を返す <see cref="Candidate"/>。
/// </summary>
public class FixedCandidate(string? text, Node node) : Candidate
{
    public override string? Text => text;
    public override Node GetNode() => node;
}
