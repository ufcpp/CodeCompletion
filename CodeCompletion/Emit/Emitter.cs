using CodeCompletion.Syntax;
using CodeCompletion.TypedText;

namespace CodeCompletion.Emit;

internal class Emitter
{
    public static ObjectMatcher? Emit(Node node, TypedToken head, ReadOnlySpan<TypedToken> tail) => node.Type switch
    {
        NodeType.Comma => new And(EmitChildren(node, head, tail)),
        NodeType.Or => new Or(EmitChildren(node, head, tail)),
        NodeType.And => new And(EmitChildren(node, head, tail)),
        NodeType.Equal or NodeType.NotEqual
            or NodeType.LessThan or NodeType.LessThanOrEqual
            or NodeType.GreaterThan or NodeType.GreaterThanOrEqual
            or NodeType.Regex
            => Compare(node, head, tail),
        _ => null, // —ˆ‚È‚¢‚Í‚¸
    };

    private static ObjectMatcher?[] EmitChildren(Node node, TypedToken head, ReadOnlySpan<TypedToken> typedTokens)
    {
        var childNodes = node.GetChildren();
        var children = new ObjectMatcher?[childNodes.Length];
        for (var i = 0; i < children.Length; ++i)
        {
            children[i] = Emit(childNodes[i], head, typedTokens);
        }
        return children;
    }

    private static ObjectMatcher? Compare(Node node, TypedToken head, ReadOnlySpan<TypedToken> typedTokens)
    {
        var compToken = typedTokens[node.Range][0];

        var valueToken = node.Right.Span[0].Span;

        var comp =
            compToken is CompareToken c ? CodeCompletion.Emit.Compare.Create(c.ComparisonType, c.Type, valueToken) :
            compToken is RegexToken ? RegexMatcher.Create(valueToken) :
            null;

        if (comp is null) return null;

        var t = new TypedTokenList(head, typedTokens);
        return MemberAccess(node.Left, t[node.Left.Range], comp);
    }

    private static ObjectMatcher? MemberAccess(Node node, TypedTokenList tokens, ObjectMatcher comp)
    {
        if (node.IsNull) return comp;

        var typedToken = tokens.Tail[0];

        if (typedToken is IntrinsicToken i)
        {
            return Intrinsic.Create(i.Name, i.SourceType, comp);
        }

        var child = MemberAccess(node.Left, tokens.Next(), comp);
        if (child is null) return null;

        var token = node.Span[0];

        if (typedToken is PrimitivePropertyToken or PropertyToken)
        {
            return new Property(token.Span.ToString(), child);
        }

        return null;
    }

    private readonly ref struct TypedTokenList(TypedToken head, ReadOnlySpan<TypedToken> tail)
    {
        public readonly TypedToken Head = head;
        public readonly ReadOnlySpan<TypedToken> Tail = tail;

        public bool IsDefault => Head is null;

        public TypedTokenList Next()
        {
            if (Tail.Length == 0) return default;

            var nextHead = Tail[0];
            return new(nextHead, Tail[1..]);
        }

        public TypedTokenList this[Range range]
        {
            get
            {
                var s = range.Start.Value;
                var head = s == 0 ? Head : Tail[s];
                var tail = Tail[range];
                return new(head, tail);
            }
        }
    }
}

