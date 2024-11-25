namespace CodeCompletion.Semantics;

/// <summary>
/// Keyword (null, true, false) と Literal (数値、文字列等) の共通基底。
/// </summary>
public abstract class ValueNode : Node
{
    public override IEnumerable<Candidate> GetCandidates(GetCandidatesContext context) => _candidates;

    private static readonly FixedCandidate[] _candidates =
    [
    ];
}
