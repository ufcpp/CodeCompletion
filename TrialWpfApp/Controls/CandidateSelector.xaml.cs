using CodeCompletion.Semantics;
using System.Windows;
using System.Windows.Controls;

namespace TrialWpfApp.Controls;

public partial class CandidateSelector : UserControl
{
    public CandidateSelector()
    {
        InitializeComponent();
    }

    public IEnumerable<Candidate> Candidates
    {
        get { return (IEnumerable<Candidate>)GetValue(CandidatesProperty); }
        set { SetValue(CandidatesProperty, value); }
    }

    public static readonly DependencyProperty CandidatesProperty =
        DependencyProperty.Register(nameof(Candidates), typeof(IEnumerable<Candidate>), typeof(CandidateSelector), new PropertyMetadata(null));

    public int SelectedIndex
    {
        get { return (int)GetValue(SelectedIndexProperty); }
        set { SetValue(SelectedIndexProperty, value); }
    }

    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(CandidateSelector), new PropertyMetadata(0));
}
