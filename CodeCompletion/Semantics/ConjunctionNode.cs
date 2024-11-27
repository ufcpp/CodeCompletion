namespace CodeCompletion.Semantics;

/// <summary>
/// And とか Or とかの共通基底。
/// </summary>
public abstract class ConjunctionNode : Node
{
    public override IEnumerable<Candidate> GetCandidates(GetCandidatesContext context)
        => context.Root.GetCandidates(context); //todo: () 対応とか
}

/// <summary>
/// or よりも結合優先度の低い and として , を使う。
/// </summary>
/// <remarks>
/// A=1 , B=2 | C=3 だと A=1 & (B=2 | C=3) の意味。
/// A=1 & B=2 | C=3 だと (A=1 & B=2) | C=3 の意味。
/// </remarks>
public class CommaNode : ConjunctionNode
{
    public override string ToString() => "and";
}

/// <summary>
/// or。
/// </summary>
public class OrNode : ConjunctionNode
{
    public override string ToString() => "Or";
}

/// <summary>
/// and。
///
/// 結合優先度高い。
/// <see cref="CommaNode"/>
/// </summary>
public class AndNode : ConjunctionNode
{
    public override string ToString() => "And";
}
