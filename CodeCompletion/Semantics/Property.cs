using System.Reflection;

namespace CodeCompletion.Semantics;


class PropertyNode(Type type) : Node
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

    public override Node GetFactory() => new PropertyNode(property.PropertyType);
}
