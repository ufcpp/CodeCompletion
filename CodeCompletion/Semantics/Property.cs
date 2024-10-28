using System.Reflection;

namespace CodeCompletion.Semantics;


class PropertyFactory(Type type) : Factory
{
    public override IEnumerable<Candidate> GetCandidates()
    {
        foreach (var property in type.GetProperties())
        {
            yield return new PropertyCandidate(property);
        }

        foreach (var comp in CompareCandidate.Singleton)
        {
            yield return comp;
        }
    }
}

class PropertyCandidate(PropertyInfo property) : Candidate
{
    public override string Text => property.Name;

    //public string? ToolTip

    public override Factory GetFactory() => new PropertyFactory(property.PropertyType);
}
