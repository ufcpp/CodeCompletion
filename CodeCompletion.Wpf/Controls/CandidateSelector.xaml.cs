using CodeCompletion.Completion;
using System.Windows;
using System.Windows.Controls;

namespace CodeCompletion.Controls;

public partial class CandidateSelector : UserControl
{
    public CandidateSelector()
    {
        InitializeComponent();
    }

    public string? Description
    {
        get { return (string?)GetValue(DescriptionProperty); }
        set { SetValue(DescriptionProperty, value); }
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(CandidateSelector), new PropertyMetadata(null));

    public IEnumerable<Candidate> Candidates
    {
        get { return (IEnumerable<Candidate>)GetValue(CandidatesProperty); }
        set { SetValue(CandidatesProperty, value); Scroll(); }
    }

    public static readonly DependencyProperty CandidatesProperty =
        DependencyProperty.Register(nameof(Candidates), typeof(IEnumerable<Candidate>), typeof(CandidateSelector), new PropertyMetadata(null));

    public int SelectedIndex
    {
        get { return (int)GetValue(SelectedIndexProperty); }
        set { SetValue(SelectedIndexProperty, value); Scroll(); }
    }

    private void Scroll()
    {
        if (Candidates?.ElementAtOrDefault(SelectedIndex) is not { } x) return;
        list.ScrollIntoView(x);
    }

    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(CandidateSelector), new PropertyMetadata(0));
}
