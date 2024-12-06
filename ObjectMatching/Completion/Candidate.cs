using CodeCompletion.Completion;
using CodeCompletion.Text;
using ObjectMatching.Reflection;
using System.Diagnostics;

namespace ObjectMatching.Completion;

//todo:
// Description みたいなの出したい。
// 整数・文字列リテラルみたいな自由入力にもヒントくらいは出したい。

internal static class Candidates
{
    public static CandidateList GetCandidates(ReadOnlySpan<char> previousToken, PropertyHierarchy property)
    {
        var nearestType = property.Nearest.PropertyType;
        if (nearestType.GetKeyValuePairType() is [_, var vt]) nearestType = vt;

        var cat = Tokenizer.Categorize(previousToken);
        if (cat == TokenCategory.Comparison)
        {
            var values =
                nearestType.Type == typeof(bool) ? _boolValues :
                property.Nearest.IsNullable ? _nullValue :
                [];
            return new(nearestType.Description, values);
        }
        if (cat == TokenCategory.DotIntrinsics && Intrinsic(previousToken, property.Nearest) is (var t, { } c))
        {
            return new(t.Description, c);
        }
        if (cat == TokenCategory.Conjunction || cat == TokenCategory.Punctuation)
        {
            // ) の後ろはリテラルとかと同じ扱い。
            if (previousToken is ")") return new(nearestType.Description, _conjunction);

            // ,|&( の後ろは前の ( 直前のプロパティを元に候補を出す。
            return new(nearestType.Description, Property(property.Parent));
        }
        if (previousToken is [] // ルート。他の判定方法の方がいいかも…
        || cat == TokenCategory.Identifier)
        {
            return new(nearestType.Description, Property(property.Nearest));
        }

        return new(nearestType.Description, _conjunction);
    }

    private static readonly Candidate[] _conjunction =
    [
        new(","),
        new("|"),
        new("&"),
    ];

    public static (TypeInfo type, IEnumerable<Candidate>? candidates) Intrinsic(ReadOnlySpan<char> token, Property property) => token switch
    {
        IntrinsicNames.Length => (new(typeof(string), property.TypeProvider), _comparableCandidates),
        IntrinsicNames.Ceiling
        or IntrinsicNames.Floor
        or IntrinsicNames.Round => (new(typeof(long), property.TypeProvider), _comparableCandidates),
        IntrinsicNames.Any or IntrinsicNames.All => ArrayIntrinsic(property),
        IntrinsicNames.Key => (property.PropertyType, Property(property)), // property にダミーでキー型のプロパティが入ってきてるはず
        _ => default,
    };

    public static IEnumerable<Candidate> Property(Property property)
        => Property(property.PropertyType, property.IsNullable).Append(new("("));

    public static IEnumerable<Candidate> Property(TypeInfo type, bool isNullable)
    {
        var x = Array(type) ?? ElementProperty(type);

        // = null, != null の分、演算子を足す。
        //todo: nullable primitive の時には重複するのではじきたい。
        if (isNullable) x = [.. x, .. _equatableCandidates];

        return x;
    }

    private static IEnumerable<Candidate>? Array(TypeInfo propertyType)
    {
        if (propertyType.GetElementType() is { } et)
        {
            var x = ElementProperty(et);
            return [.. x, .. _arrayIntrinsics];
        }
        return null;
    }

    private static (TypeInfo elementType, IEnumerable<Candidate>? candidates) ArrayIntrinsic(Property property)
    {
        // Array と違って、 .any .all は出さない。
        var et0 = property.PropertyType.GetElementType();
        Debug.Assert(et0 != null);
        var et = et0.GetValueOrDefault();
        var x = ElementProperty(et);
        return (et, [.. x, new("(")]);
    }

    private static IEnumerable<Candidate> ElementProperty(TypeInfo type)
        => PrimitiveProperty(type)
            ?? GetProperties(type);

    private static IEnumerable<Candidate> GetProperties(TypeInfo type)
        => GetKeyValuePairProperties(type)
            ?? type.GetProperties().Select(p => new Candidate(p.Name, p.Description));

    private static IEnumerable<Candidate>? GetKeyValuePairProperties(TypeInfo type)
    {
        if (type.GetKeyValuePairType() is not [_, var vt]) return null;

        // ここの isNullable、ちゃんと NullabilityInfoContext 追えば取れるんだけど、
        // 今の型構造的に伝搬が難しくて悩み中。
        var xx = Property(vt, false);
        return [.. Property(vt, false), _keyCandidate];
    }

    private static Candidate[]? PrimitiveProperty(TypeInfo type)
    {
        var t = type.Type;
        if (Nullable.GetUnderlyingType(t) is { } ut) t = ut;

        return t.GetComparableType() switch
        {
            ComparableTypeCategory.String => _stringCandidates,
            ComparableTypeCategory.Bool => _boolCandidates,
            ComparableTypeCategory.Float => _floatCandidates,
            ComparableTypeCategory.Integer => _comparableCandidates,
            ComparableTypeCategory.Enum => _comparableCandidates,
            ComparableTypeCategory.Comparable => [.. _comparableCandidates, .. GetProperties(type)],
            ComparableTypeCategory.Equatable => [.. _equatableCandidates, .. GetProperties(type)],
            _ => null,
        };
    }

    private static readonly Candidate[] _nullValue = [new("null")];
    private static readonly Candidate[] _boolValues = [new("true"), new("false")];

    private static readonly Candidate[] _equatableCandidates =
    [
        new("="),
        new("!="),
    ];

    private static readonly Candidate[] _boolCandidates = _equatableCandidates;
    // bool にも一応 ( 出す？

    private static readonly Candidate[] _comparableCandidates =
    [
        new("="),
        new("!="),
        new("<"),
        new("<="),
        new(">"),
        new(">="),
    ];

    private static readonly Candidate _keyCandidate = new(IntrinsicNames.Key, IntrinsicDescription.Key);

    private static readonly Candidate[] _stringCandidates =
    [
        .._comparableCandidates,
        new("~"),
        new(IntrinsicNames.Length, IntrinsicDescription.StringLength),
    ];

    private static readonly Candidate[] _floatCandidates =
    [
        .._comparableCandidates,
        new(IntrinsicNames.Ceiling),
        new(IntrinsicNames.Floor),
        new(IntrinsicNames.Round),
    ];

    private static readonly Candidate[] _arrayIntrinsics =
    [
        new(IntrinsicNames.Any, IntrinsicDescription.Any),
        new(IntrinsicNames.All, IntrinsicDescription.All),
        new(IntrinsicNames.Length, IntrinsicDescription.ArrayLength),
    ];
}
