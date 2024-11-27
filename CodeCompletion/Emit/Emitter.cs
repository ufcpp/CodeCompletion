using CodeCompletion.Syntax;
using CodeCompletion.TypedText;

namespace CodeCompletion.Emit;

internal class Emitter
{
    public static ObjectMatcher? Emit(Node node, TypedToken head, ReadOnlySpan<TypedToken> tail) => node.Type switch
    {
        NodeType.Term => Emit(new EmitContext(head, tail[node.Range], node.Span)),
        NodeType.Comma => new And(EmitChildren(node, head, tail)),
        NodeType.Or => new Or(EmitChildren(node, head, tail)),
        NodeType.And => new And(EmitChildren(node, head, tail)),
        _ => null,
    };

    private static ObjectMatcher[] EmitChildren(Node node, TypedToken head, ReadOnlySpan<TypedToken> typedTokens)
    {
        var childNodes = node.GetChildren();
        var children = new ObjectMatcher[childNodes.Length];
        for (var i = 0; i < children.Length; ++i)
        {
            children[i] = Emit(childNodes[i], head, typedTokens)!;
        }
        return children;
    }

    public static ObjectMatcher? Emit(EmitContext context)
    {
        if (context.IsDefault) return null;

        if (context.Head is PropertyToken)
        {
            var next = context.Next();
            if (next.Head is CompareToken c)
            {
                var next2 = next.Next();
                if (next2.Head is not KeywordToken { Keyword: "null" }) return null;

                return c.ComparisonType switch
                {
                    ComparisonType.Equal => Compare.IsNull,
                    ComparisonType.NotEqual => Compare.IsNotNull,
                    _ => null
                };
            }

            var matcher = Emit(next);
            if (matcher is null) return null;
            return new Property(context.Token.Span.ToString(), matcher);
        }

        if (context.Head is PrimitivePropertyToken p)
        {
            var next = context.Next();

            if (next.Head is IntrinsicToken intrinsic)
            {
                var matcher = Emit(next);
                if (matcher is null) return null;
                return Intrinsic.Create(intrinsic.Name, intrinsic.SourceType, matcher);
            }
            else if (next.Head is RegexToken)
            {
                if (next.Next().Head is not LiteralToken) return null;
                return RegexMatcher.Create(next.Token.Span);
            }

            if (next.Head is not CompareToken c) return null;

            var next2 = next.Next();

            if (next2.Head is KeywordToken { Keyword: var keyword })
            {
                return keyword switch
                {
                    "true" => Compare<bool>.Create(c.ComparisonType, true),
                    "false" => Compare<bool>.Create(c.ComparisonType, false),
                    _ => null
                };
            }

            if (next2.Head is not LiteralToken) return null;

            return Compare.Create(c.ComparisonType, p.Type, next.Token.Span);
        }

        return null;
    }
}

