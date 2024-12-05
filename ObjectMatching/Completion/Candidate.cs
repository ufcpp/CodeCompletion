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
        var cat = Tokenizer.Categorize(previousToken);
        if (cat == TokenCategory.Comparison)
        {
            if (property.Nearest.PropertyType.Type == typeof(bool))
            {
                return new("", _boolValues);
            }
            if (property.Nearest.IsNullable)
            {
                return new("", _nullValue);
            }
            return new("", []); //todo: 何型の自由入力かのヒントくらいは出せるようにしたい。
        }
        if (cat == TokenCategory.DotIntrinsics && Intrinsic(previousToken, property.Nearest) is { } c)
        {
            return new("", c);
        }
        if (cat == TokenCategory.Conjunction || cat == TokenCategory.Punctuation)
        {
            // ) の後ろはリテラルとかと同じ扱い。
            if (previousToken is ")") return new("", _conjunction);

            // ,|&( の後ろは前の ( 直前のプロパティを元に候補を出す。
            return new("", Property(property.Parent));
        }
        if (previousToken is [] // ルート。他の判定方法の方がいいかも…
        || cat == TokenCategory.Identifier)
        {
            return new("", Property(property.Nearest));
        }

        return new("", _conjunction);
    }

    private static readonly Candidate[] _conjunction =
    [
        new(","),
        new("|"),
        new("&"),
    ];

    public static IEnumerable<Candidate>? Intrinsic(ReadOnlySpan<char> token, Property property) => token switch
    {
        IntrinsicNames.Length
        or IntrinsicNames.Ceiling
        or IntrinsicNames.Floor
        or IntrinsicNames.Round => _comparableCandidates,
        IntrinsicNames.Any or IntrinsicNames.All => ArrayIntrinsic(property),
        _ => null,
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

    private static IEnumerable<Candidate>? ArrayIntrinsic(Property property)
    {
        // Array と違って、 .any .all は出さない。
        var et = property.PropertyType.GetElementType();
        Debug.Assert(et != null);
        var x = ElementProperty(et.GetValueOrDefault());
        return [.. x, new("(")];
    }

    private static IEnumerable<Candidate> ElementProperty(TypeInfo type)
        => PrimitiveProperty(type)
            ?? GetProperties(type);

    private static IEnumerable<Candidate> GetProperties(TypeInfo type)
        => type.GetProperties().Select(p => new Candidate(p.Name));

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

    private static readonly Candidate[] _stringCandidates =
    [
        .._comparableCandidates,
        new("~"),
        new(IntrinsicNames.Length),
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
        new(IntrinsicNames.Any),
        new(IntrinsicNames.All),
        new(IntrinsicNames.Length),
    ];
}
