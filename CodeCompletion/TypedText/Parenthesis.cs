namespace CodeCompletion.TypedText;

internal class Parenthesis
{
    public static readonly FixedCandidate Open = new("(", new OpenParenToken());
    public static readonly FixedCandidate Close = new(")", new CloseParenToken());
}

public class OpenParenToken : TypedToken
{
    // ConjunctionToken と共通化する？
    public override IEnumerable<Candidate> GetCandidates(GetCandidatesContext context)
        => context.Root.GetCandidates(context).Append(Parenthesis.Open); //todo: token(comma_expression) 対応とか

    public override string ToString() => "(";
}

public class CloseParenToken : TypedToken
{
    // ValueToken と共通化する？
    public override IEnumerable<Candidate> GetCandidates(GetCandidatesContext context)
        => ValueToken._candidates;

    public override string ToString() => ")";
}
