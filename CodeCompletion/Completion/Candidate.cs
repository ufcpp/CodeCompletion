using CodeCompletion.Text;
using System.Diagnostics;

namespace CodeCompletion.Completion;

/// <summary>
/// 補完候補。
/// </summary>
public record struct Candidate(string? Text);

//todo:
// Description みたいなの出したい。
// 整数・文字列リテラルみたいな自由入力にもヒントくらいは出したい。

internal static class Candidates
{
    public static IEnumerable<Candidate> GetCandidates(ReadOnlySpan<char> previousToken, PropertyHierarchy property)
    {
        var cat = Tokenizer.Categorize(previousToken);
        if (cat == TokenCategory.Operator)
        {
            if (property.Direct.PropertyType == typeof(bool))
            {
                return _boolValues;
            }
            if (property.Direct.IsNullable)
            {
                return _nullValue;
            }
            return [new(null)]; //todo: 何型の自由入力かのヒントくらいは出せるようにしたい。
        }
        if (cat == TokenCategory.DotIntrinsics && Intrinsic(previousToken, property.Direct) is { } c)
        {
            return c;
        }
        if (cat == TokenCategory.Isolation)
        {
            // ) の後ろはリテラルとかと同じ扱い。
            if (previousToken is ")") return _conjunction;

            // ,|&( の後ろは前の ( 直前のプロパティを元に候補を出す。
            return Property(property.Parenthesis);
        }
        if (previousToken is [] // ルート。他の判定方法の方がいいかも…
        || cat == TokenCategory.Identifier)
        {
            return Property(property.Direct);
        }

        return _conjunction;

        //todo: 色々移植。
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
        if (property.IsNullable) x = [..x, .._equatableCandidates];

        return x.Append(new("("));

        //todo: enumerable
    }

    private static IEnumerable<Candidate>? Array(Property property)
    {
        if (TypeHelper.GetElementType(property.PropertyType) is { } et)
        {
            var x = ElementProperty(et);
            return [.. x, .. _arrayIntrinsics];
        }
        return null;
    }

    private static IEnumerable<Candidate>? ArrayIntrinsic(Property property)
    {
        // Array と違って、 .any .all は出さない。
        var et = TypeHelper.GetElementType(property.PropertyType);
        Debug.Assert(et is not null);
        var x = ElementProperty(et);
        return [.. x, new("(")];
    }

    private static IEnumerable<Candidate> ElementProperty(Type type)
        => PrimitiveProperty(type)
            ?? type.GetProperties().Select(p => new Candidate(p.Name));

    private static Candidate[]? PrimitiveProperty(Type type)
    {
        if (type == typeof(string)) return _stringCandidates;
        if (type == typeof(bool)) return _boolCandidates;
        if (type == typeof(float)
            || type == typeof(double)
            || type == typeof(decimal)
            ) return _floatCandidates;
        if (type == typeof(int)
            || type == typeof(long)
            || type == typeof(short)
            || type == typeof(byte)
            || type == typeof(uint)
            || type == typeof(ulong)
            || type == typeof(ushort)
            || type == typeof(sbyte)
            || type == typeof(TimeSpan)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(DateOnly)
            || type == typeof(TimeOnly)
            ) return _comparableCandidates;

        //todo: nullable
        //todo: enum

        //todo: IComparable かつ ISpanParseable
        //todo: IEquatable かつ ISpanParseable

        return null;
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
