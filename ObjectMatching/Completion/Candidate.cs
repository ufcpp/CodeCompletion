using CodeCompletion.Completion;
using CodeCompletion.Text;
using ObjectMatching.Reflection;
using System.Diagnostics;

namespace ObjectMatching.Completion;

//todo:
// Description みたいなの出したい。
// 整数・文字列リテラルみたいな自由入力にもヒントくらいは出したい。

internal static partial class Candidates
{
    public static CandidateList GetCandidates(ReadOnlySpan<char> previousToken, PropertyHierarchy property)
    {
        var cat = Tokenizer.Categorize(previousToken);
        if (cat == TokenCategory.Comparison)
        {
            if (property.Nearest.PropertyType.Type.IsEnum)
            {
                return GetEnumCandidates(property.Nearest.PropertyType);
            }
            var values =
                property.Nearest.PropertyType.Type == typeof(bool) ? _boolValues :
                property.Nearest.IsNullable ? _nullValue :
                [];
            return new(property.Nearest.PropertyType.Description, values);
        }
        if (cat == TokenCategory.DotIntrinsics && Intrinsic(previousToken, property.Nearest) is (var t, { } c))
        {
            return new(t.Description, c);
        }
        if (cat == TokenCategory.Conjunction || cat == TokenCategory.Punctuation)
        {
            // ) の後ろはリテラルとかと同じ扱い。
            if (previousToken is ")") return new(property.Nearest.PropertyType.Description, _conjunction);

            // ,|&( の後ろは前の ( 直前のプロパティを元に候補を出す。
            return new(property.Nearest.PropertyType.Description, Property(property.Parent));
        }
        if (previousToken is [] // ルート。他の判定方法の方がいいかも…
        || cat == TokenCategory.Identifier)
        {
            return new(property.Nearest.PropertyType.Description, Property(property.Nearest));
        }

        return new(property.Nearest.PropertyType.Description, _conjunction);
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
        _ => default,
    };

    public static IEnumerable<Candidate> Property(Property property)
    {
        var x = Array(property) ?? ElementProperty(property.PropertyType);

        // = null, != null の分、演算子を足す。
        //todo: nullable primitive の時には重複するのではじきたい。
        if (property.IsNullable) x = [.. x, .. _equatableCandidates];

        return x.Append(new("("));
    }

    private static IEnumerable<Candidate>? Array(Property property)
    {
        if (property.PropertyType.GetElementType() is { } et)
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
        => type.GetProperties().Select(p => new Candidate(p.Name, p.Description));

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
            ComparableTypeCategory.Enum => IsFlagsEnum(t) ? _flagsEnumCandidates : _comparableCandidates,
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

    private static readonly Candidate[] _comparableCandidates =
    [
        new("="),
        new("!="),
        new("<"),
        new("<="),
        new(">"),
        new(">="),
    ];

    private static readonly Candidate[] _flagsEnumCandidates =
    [
        new("="),
        new("~", "HasFlag"),
        new("!="),
        new("<"),
        new("<="),
        new(">"),
        new(">="),
    ];

    private static readonly Candidate[] _stringCandidates =
    [
        .._comparableCandidates,
        new("~", "正規表現"),
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
