namespace CodeCompletion.Syntax;

public abstract class Candidate
{
    public abstract string Text { get; }
    public abstract Factory GetFactory();
}
