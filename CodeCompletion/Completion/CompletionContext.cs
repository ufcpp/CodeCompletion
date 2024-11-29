using CodeCompletion.Emit;
using CodeCompletion.Text;

namespace CodeCompletion.Completion;

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

    /// <summary>
    /// 全更新。
    /// todo: 1ストロークごとに更新処理掛けれないか要検討。
    /// </summary>
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
            var t = property.Direct.PropertyType;
            if (TypeHelper.GetElementType(t) is { } et) t = et;
            var p = t.GetProperty(text.ToString());
            if (p is null) return null;
            return new(p);
        }

        if (cat == TokenCategory.DotIntrinsics)
        {
            if (text is IntrinsicNames.Any or IntrinsicNames.All) return property.Direct;

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

    public void GetCandidates(IList<Candidate> results)
    {
        results.Clear();
        var (pos, _) = Texts.GetPosition();
        var previousToken = pos == 0 ? "" : Texts.Tokens[pos - 1].Span;
        var text = Texts.Tokens[pos].Span;

        foreach (var candidate in Candidates.GetCandidates(previousToken, _propertyInfo[pos]))
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

