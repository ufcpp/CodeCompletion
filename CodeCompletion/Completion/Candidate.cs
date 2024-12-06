namespace CodeCompletion.Completion;

/// <summary>
/// 補完候補。
/// </summary>
public record struct Candidate(string Text, string? Description = null);

/// <summary>
/// 補完候補リスト。
/// 「int の時は候補なし、整数と説明だけ出したい」みたいなのがあり。
/// </summary>
/// <param name="Description">説明。</param>
/// <param name="Candidates">候補一覧。</param>
public record struct CandidateList(string? Description, IEnumerable<Candidate> Candidates);
