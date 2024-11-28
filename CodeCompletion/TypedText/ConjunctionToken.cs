namespace CodeCompletion.TypedText;

/// <summary>
/// And とか Or とかの共通基底。
/// </summary>
public abstract class ConjunctionToken : TypedToken
{
    public override IEnumerable<Candidate> GetCandidates(PropertyTokenBase context)
        => context.GetCandidates(context);
}

/// <summary>
/// or よりも結合優先度の低い and として , を使う。
/// </summary>
/// <remarks>
/// A=1 , B=2 | C=3 だと A=1 & (B=2 | C=3) の意味。
/// A=1 & B=2 | C=3 だと (A=1 & B=2) | C=3 の意味。
/// </remarks>
public class CommaToken : ConjunctionToken
{
    public override string ToString() => "and";
}

/// <summary>
/// or。
/// </summary>
public class OrToken : ConjunctionToken
{
    public override string ToString() => "Or";
}

/// <summary>
/// and。
///
/// 結合優先度高い。
/// <see cref="CommaToken"/>
/// </summary>
public class AndToken : ConjunctionToken
{
    public override string ToString() => "And";
}
