namespace CodeCompletion.TypedText;

/// <summary>
/// Keyword (null, true, false) と Literal (数値、文字列等) の共通基底。
/// </summary>
public abstract class ValueToken : TypedToken
{
    public override IEnumerable<Candidate> GetCandidates(PropertyTokenBase context) => _candidates;

    internal static readonly FixedCandidate[] _candidates =
    [
        new(",", new CommaToken()),
        new("|", new OrToken()),
        new("&", new AndToken()),
        Parenthesis.Close, //todo: 対応する開きカッコがあるときだけ出すとかやる？
    ];
}
