using CodeCompletion.Text;

namespace CodeCompletion.Completion;

public interface ICompletionContext
{
    TextBuffer Texts { get; }
    void GetCandidates(IList<Candidate> candidates);
    void Refresh();
}
