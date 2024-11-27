namespace CodeCompletion.TypedText;

/// <summary>
/// Keyword (null, true, false) と Literal (数値、文字列等) の共通基底。
/// </summary>
public abstract class ValueToken : TypedToken
{
    public override IEnumerable<Candidate> GetCandidates(GetCandidatesContext context) => _candidates;

    private static readonly FixedCandidate[] _candidates =
    [
        new(",", new CommaToken()),
        new("|", new OrToken()),
        new("&", new AndToken()),
    ];
}
