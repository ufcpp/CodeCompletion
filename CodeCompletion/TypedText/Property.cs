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

        var t = Type;
        bool isArray = t.IsArray;

        if (isArray) //todo: IList? IEnumerable?
        {
            t = t.GetElementType()!;
        }

        foreach (var property in t.GetProperties())
        {
            yield return new PropertyCandidate(property);
        }

        if (isArray)
        {
            yield return new FixedCandidate(IntrinsicNames.Length, new IntrinsicToken(IntrinsicNames.Length, Type, typeof(int)));
            yield return new FixedCandidate(IntrinsicNames.Any, new ArrayAnyToken(this));
            yield return new FixedCandidate(IntrinsicNames.All, new ArrayAllToken(this));
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
        //todo: ISpanParseable かつ IComparable or IEquatable なものに対応
        // 無差別に判定して PrimitivePropertyToken に流していい？困る？
        // (今はコード補完だけ聞かない。Parse, Emit はできてる。)
        var t = property.PropertyType;
        return (TypedToken?)PrimitivePropertyToken.Get(t) ?? new PropertyToken(property);
    }
}
