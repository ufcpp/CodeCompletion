using CodeCompletion.Text;

namespace CodeCompletion.Completion;

public interface ICompletionContext
{
    IEnumerable<Candidate> GetCandidates(ReadOnlySpan<char> previousToken, int tokenPosition);
    void Refresh(TextBuffer texts);
}
