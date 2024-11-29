using CodeCompletion.Emit;
using CodeCompletion.Text;

namespace CodeCompletion.TypedText;

public class CompletionContext
{
    public CompletionContext(Type root, TextBuffer texts)
    {
        Root = root;
        Texts = texts;
        Refresh();
    }

    public Type Root { get; }
    public TextBuffer Texts { get; }

    private readonly List<PropertyHierarchy> _propertyInfo = [];

    public void Refresh()
    {
        var t = Root;

        _propertyInfo.Clear();

        Property property = new(t, "", false);
        var stack = new Stack<Property>();
        stack.Push(property);
        _propertyInfo.Add(new(property, property));

        var tokens = Texts.Tokens;

        for (int i = 0; i < tokens.Length; i++)
        {
            var text = tokens[i].Span;

            if (text is "(")
            {
                stack.Push(property);
            }
            else if (text is ")")
            {
                stack.Pop();
            }
            else if (GetProperty(text, _propertyInfo[^1]) is { } p)
            {
                property = p;
            }
            _propertyInfo.Add(new(property, stack.Peek()));
        }
    }

    private static Property? GetProperty(ReadOnlySpan<char> text, PropertyHierarchy property)
    {
        var cat = Tokenizer.Categorize(text);
        if (cat == TokenCategory.Identifier)
        {
            var p = property.Direct.PropertyType.GetProperty(text.ToString());
            if (p is null) return null;
            return new(p);
        }

        if (cat == TokenCategory.DotIntrinsics)
        {
            var t = text switch
            {
                IntrinsicNames.Length => typeof(int),
                IntrinsicNames.Ceiling => typeof(long),
                IntrinsicNames.Floor => typeof(long),
                IntrinsicNames.Round => typeof(long),
                _ => null,
            };

            if (t is null) return null;
            return new(t, text.ToString(), false);
        }

        return null;
    }

    private static IEnumerable<Candidate> GetCandidates(ReadOnlySpan<char> previousToken, PropertyHierarchy property)
    {
        var cat = Tokenizer.Categorize(previousToken);
        if (cat == TokenCategory.Operator)
        {
            if (property.Direct.PropertyType == typeof(bool))
            {
                return [new("true"), new("false")];
            }
            // nullable

            else return [new(null)]; //todo: 何型の自由入力かのヒントくらいは出せるようにしたい。
        }
        if (cat == TokenCategory.DotIntrinsics)
        {
            return previousToken switch
            {
                IntrinsicNames.Length => _stringCandidates,
                IntrinsicNames.Ceiling
                or IntrinsicNames.Floor
                or IntrinsicNames.Round => _comparableCandidates,
                _ => [],
            };
        }
        if (cat == TokenCategory.Isolation)
        {
            // ) の後ろはリテラルとかと同じ扱い。
            if (previousToken is ")") return _conjunction;

            // ,|&( の後ろは前の ( 直前のプロパティを元に候補を出す。
            return GetPropertyCandidates(property.Parenthesis);
        }
        if (previousToken is [] // ルート。他の判定方法の方がいいかも…
        || cat == TokenCategory.Identifier)
        {
            return GetPropertyCandidates(property.Direct);
        }

        return _conjunction;

        //todo: 色々移植。
    }

    private static IEnumerable<Candidate> GetPropertyCandidates(Property property)
    {
        if (property.PropertyType == typeof(string)) return _stringCandidates;
        if (property.PropertyType == typeof(bool)) return _boolCandidates;
        if (property.PropertyType == typeof(float)
            || property.PropertyType == typeof(double)
            || property.PropertyType == typeof(decimal)
            ) return _floatCandidates;
        if (property.PropertyType == typeof(int)
            || property.PropertyType == typeof(long)
            || property.PropertyType == typeof(short)
            || property.PropertyType == typeof(byte)
            || property.PropertyType == typeof(uint)
            || property.PropertyType == typeof(ulong)
            || property.PropertyType == typeof(ushort)
            || property.PropertyType == typeof(sbyte)
            || property.PropertyType == typeof(TimeSpan)
            || property.PropertyType == typeof(DateTime)
            || property.PropertyType == typeof(DateTimeOffset)
            || property.PropertyType == typeof(DateOnly)
            || property.PropertyType == typeof(TimeOnly)
            ) return _comparableCandidates;
        //todo: IComparable かつ ISpanParseable
        //todo: IEquatable かつ ISpanParseable

        return property.PropertyType.GetProperties().Select(p => new Candidate(p.Name)).Append(new("("));
    }

    private static readonly Candidate[] _conjunction =
    [
        new(","),
        new("|"),
        new("&"),
    ];

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

    public void GetCandidates(IList<Candidate> results)
    {
        results.Clear();
        var (pos, _) = Texts.GetPosition();
        var previousToken = pos == 0 ? "" : Texts.Tokens[pos - 1].Span;
        var text = Texts.Tokens[pos].Span;

        foreach (var candidate in GetCandidates(previousToken, _propertyInfo[pos]))
        {
            if (candidate.Text is not { } ct
                || ct.AsSpan().StartsWith(text, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(candidate);
            }
        }
    }

    public void Reset(ReadOnlySpan<char> source)
    {
        Texts.Reset(source);
        Refresh();
    }

    public Func<object?, bool>? Emit() => Emitter.Emit(Texts, Root)!;
}

