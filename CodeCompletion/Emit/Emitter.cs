using CodeCompletion.Semantics;

namespace CodeCompletion.Emit;

internal class Emitter
{
    public static ObjectMatcher? Emit(EmitContext context)
    {
        if (context.IsDefault) return null;

        if (context.Head is PropertyNode)
        {
            var next = context.Next();
            if (next.Head is CompareNode c)
            {
                var next2 = next.Next();
                if (next2.Head is not KeywordNode { Keyword: "null" }) return null;

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

        if (context.Head is PrimitivePropertyNode p)
        {
            var next = context.Next();

            if (next.Head is IntrinsicNode intrinsic)
            {
                var matcher = Emit(next);
                if (matcher is null) return null;
                return Intrinsic.Create(intrinsic.Name, intrinsic.SourceType, matcher);
            }
            else if (next.Head is RegexNode)
            {
                if (next.Next().Head is not LiteralNode) return null;
                return RegexMatcher.Create(next.Token.Span);
            }

            if (next.Head is not CompareNode c) return null;

            var next2 = next.Next();

            if (next2.Head is KeywordNode { Keyword: var keyword })
            {
                return keyword switch
                {
                    "true" => Compare<bool>.Create(c.ComparisonType, true),
                    "false" => Compare<bool>.Create(c.ComparisonType, false),
                    _ => null
                };
            }

            if (next2.Head is not LiteralNode) return null;

            return Compare.Create(c.ComparisonType, p.Type, next.Token.Span);
        }

        return null;
    }
}

