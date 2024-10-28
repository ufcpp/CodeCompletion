namespace CodeCompletion.Semantics;

public abstract class Candidate
{
    public abstract string Text { get; }
    public abstract Node GetNode();
}
