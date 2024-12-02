using ObjectMatching.Completion;
using ObjectMatching.Reflection;
using System.Collections;

namespace TrialWpfApp;

public class ViewModel(IEnumerable itemsSource, ITypeProvider? typeProvider = null)
    : CodeCompletion.ViewModels.ViewModel<CompletionContext>(new(new(GetElementType(itemsSource), typeProvider ?? new DefaultTypeProvider()), new()), itemsSource)
{
    private static Type GetElementType(object obj)
    {
        var t = obj.GetType();
        if (t.IsArray) return t.GetElementType()!;

        foreach (var i in t.GetInterfaces())
        {
            if (!i.IsGenericType) continue;

            var gt = t.GetGenericTypeDefinition();

            if (gt == typeof(IEnumerable<>)) return i.GetGenericArguments()[0];
        }

        throw new InvalidOperationException();
    }

    public override void Refresh()
    {
        base.Refresh();
        ShowDiag();
    }

    private void ShowDiag()
    {
        var buffer = Texts;
        var (t, p) = buffer.GetPosition();
        System.Diagnostics.Debug.WriteLine($"""
cursor: {buffer.Cursor} token: {t} pos: {p}
candidates: {string.Join(", ", Candidates.Select(x => x.Text))} (selected: {SelectedCandidateIndex})

""");
    }

    protected override IEnumerable Filter(IEnumerable itemsSource)
    {
        var filter = Context.Emit();

        if (filter is null)
        {
            System.Diagnostics.Debug.WriteLine("filter OFF");
            return itemsSource;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("filter ON");
            return itemsSource.Cast<object>().Where(filter).ToList();
        }
    }
}
