using CodeCompletion.Text;

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
            // nullable

            else return [new(null)]; //todo: 何型の自由入力かのヒントくらいは出せるようにしたい。
        }
        if (cat == TokenCategory.DotIntrinsics && Intrinsic(previousToken) is { } c)
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

    public static IEnumerable<Candidate>? Intrinsic(ReadOnlySpan<char> token) => token switch
    {
        IntrinsicNames.Length => _stringCandidates,
        IntrinsicNames.Ceiling
        or IntrinsicNames.Floor
        or IntrinsicNames.Round => _comparableCandidates,
        //todo: .any .all
        _ => null,
    };

    public static IEnumerable<Candidate> Property(Property property)
    {
        //todo: enumerable

        return PrimitiveProperty(property.PropertyType)
            ?? property.PropertyType.GetProperties().Select(p => new Candidate(p.Name)).Append(new("("));
    }

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

    private static readonly Candidate[] _boolValues = [new("true"), new("false")];

    private static readonly Candidate[] _boolCandidates =
    [
        new("="),
        new("!="),
        // bool にも一応 ( 出す？
        // その場合、任意の equatable と同じ扱いなので _equatableCandidates って名前にする？
    ];

    private static readonly Candidate[] _comparableCandidates =
    [
        new("="),
        new("!="),
        new("<"),
        new("<="),
        new(">"),
        new(">="),
        new("("), // 末尾に持っていきたい
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
}
