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
    }

    public override string ToString() => $"Property {type.Name}";
}

class PropertyCandidate(PropertyInfo property) : Candidate
{
    public override string Text => property.Name;

    //public string? ToolTip

    public override Node GetNode()
    {
        var t = property.PropertyType;

        if (t == typeof(int)
            || t == typeof(long)
            || t == typeof(byte)
            || t == typeof(short)
            || t == typeof(uint)
            || t == typeof(ulong)
            || t == typeof(ushort)
            || t == typeof(sbyte)
            ) return PrimitivePropertyNode.Integer;

        if (t == typeof(float)
            || t == typeof(double)
            ) return PrimitivePropertyNode.Float;

        if (t == typeof(string)) return PrimitivePropertyNode.String;

        return new PropertyNode(property.PropertyType);
    }
}
