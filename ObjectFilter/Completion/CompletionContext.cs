using CodeCompletion.Completion;
using CodeCompletion.Text;
using ObjectFilter.Emit;
using ObjectFilter.Reflection;

namespace ObjectFilter.Completion;

public class CompletionContext(TypeInfo root) : ICompletionContext
{
    public TypeInfo Root { get; } = root;

    public CompletionContext(Type root, ITypeProvider typeProvider) : this(new(root, typeProvider)) { }

    private readonly List<PropertyHierarchy> _propertyInfo = [];

    /// <summary>
    /// 全更新。
    /// todo: 1ストロークごとに更新処理掛けれないか要検討。
    /// </summary>
    public void Refresh(TextBuffer texts)
    {
        var t = Root;

        _propertyInfo.Clear();

        Property property = new(t, "", false);
        var parent = new Stack<Property>();
        parent.Push(property);
        _propertyInfo.Add(new(property, property));

        var tokens = texts.Tokens;

        for (int i = 0; i < tokens.Length; i++)
        {
            var text = tokens[i].Span;

            if (text is "(")
            {
                parent.Push(property);
            }
            else if (text is ")" && parent.Count > 1)
            {
                property = parent.Pop();
            }
            else if (text is "," or "|" or "&")
            {
                property = parent.Peek();
            }
            else if (GetProperty(text, _propertyInfo[^1]) is { } p)
            {
                property = p;
            }
            _propertyInfo.Add(new(parent.Peek(), property));
        }
    }

    private static Property? GetProperty(ReadOnlySpan<char> text, PropertyHierarchy property)
    {
        var cat = Tokenizer.Categorize(text);
        if (cat == TokenCategory.Identifier)
        {
            var t = property.Nearest.PropertyType;
            if (t.GetElementType() is { } et) t = et;
            return t.GetProperty(text.ToString()) is { } p ? new(p) : null;
        }

        if (cat == TokenCategory.DotIntrinsics)
        {
            if (text is IntrinsicNames.Any or IntrinsicNames.All) return property.Nearest with { Name = text.ToString() };

            var t = text switch
            {
                IntrinsicNames.Length => typeof(int),
                IntrinsicNames.Ceiling => typeof(long),
                IntrinsicNames.Floor => typeof(long),
                IntrinsicNames.Round => typeof(long),
                _ => null,
            };

            if (t is null) return null;
            return new(new(t, property.TypeProvider), text.ToString(), false);
        }

        return null;
    }

    public CandidateList GetCandidates(ReadOnlySpan<char> previousToken, int tokenPosition)
    {
        return Candidates.GetCandidates(previousToken, _propertyInfo[tokenPosition]);
    }

    public Result<Func<object?, bool>, Error> Emit(TextBuffer texts) => Emitter.Emit(texts, Root)!;
}

