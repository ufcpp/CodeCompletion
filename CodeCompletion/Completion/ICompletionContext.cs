using CodeCompletion.Text;

namespace CodeCompletion.Completion;

public interface ICompletionContext
{
    CandidateList GetCandidates(ReadOnlySpan<char> previousToken, int tokenPosition);
    void Refresh(TextBuffer texts);
}
