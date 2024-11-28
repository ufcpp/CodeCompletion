using System.Reflection;

namespace CodeCompletion.TypedText;

public abstract class PropertyTokenBase : TypedToken;

public class PropertyToken(Type type, bool isNullable = false) : PropertyTokenBase
{
    public Type Type { get; } = type;

    public PropertyToken(PropertyInfo p) : this(p.PropertyType, IsNullable(p)) { }

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

    public override IEnumerable<Candidate> GetCandidates(PropertyTokenBase context)
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

        yield return Parenthesis.Open;
    }

    public override string ToString() => $"Property {Type.Name}";
}

public class PropertyCandidate(PropertyInfo property) : Candidate
{
    public override string? Text => property.Name;

    //public string? ToolTip

    public override TypedToken GetToken()
    {
        var t = property.PropertyType;
        return (TypedToken?)PrimitivePropertyToken.Get(t) ?? new PropertyToken(property);
    }
}
