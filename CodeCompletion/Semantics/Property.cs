using System.Reflection;

namespace CodeCompletion.Semantics;

class PropertyNode(Type type, bool isNullable = false) : Node
{
    public Type Type { get; } = type;

    public PropertyNode(PropertyInfo p) : this(p.PropertyType, IsNullable(p)) { }

    private static bool IsNullable(PropertyInfo p)
    {
        var pt = p.PropertyType;

        // 値型
        if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Nullable<>)) return true;

        // T? じゃない値型
        if (pt.IsValueType) return false;

        // 参照型
        var c = new NullabilityInfoContext();
        var i = c.Create(p);
        return i.ReadState != NullabilityState.NotNull;
    }

    public override IEnumerable<Candidate> GetCandidates()
    {
        if (isNullable)
        {
            yield return new CompareCandidate(typeof(object), ComparisonType.Equal);
            yield return new CompareCandidate(typeof(object), ComparisonType.NotEqual);
        }

        foreach (var property in Type.GetProperties())
        {
            yield return new PropertyCandidate(property);
        }

        //todo: is null, is not null
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
        return (Node?)PrimitivePropertyNode.Get(t) ?? new PropertyNode(property);
    }
}
