using System.Reflection;

namespace CodeCompletion.Semantics;

class PropertyNode(Type type) : Node
{
    public Type Type { get; } = type;

    public override IEnumerable<Candidate> GetCandidates()
    {
        foreach (var property in Type.GetProperties())
        {
            yield return new PropertyCandidate(property);
        }
    }

    public override string ToString() => $"Property {Type.Name}";
}

class PropertyCandidate(PropertyInfo property) : Candidate
{
    public override string? Text => property.Name;

    //public string? ToolTip

    public override Node GetNode()
    {
        var t = property.PropertyType;
        return PrimitivePropertyNode.Get(t);
    }
}
