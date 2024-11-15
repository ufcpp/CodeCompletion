using CodeCompletion.Semantics;

namespace CodeCompletion.Emit;

public class Emitter
{
    // Node 自体にインスタンスメソッドで持たせる？
    // その場合、「Compare は後ろの Literal を参照する」みたいなのとか、
    // AND/OR をどうやろう？
    // やっぱ AST なレイヤー要る？

    // エラーどうしよう？
    // Func or Error な union 返す？
    // 例外？
    public static Func<object?, bool>? Emit(EmitContext context)
    {
        var matcher = EmitInternal(context);

        if (matcher is null)
        {
            // 例外？
            // Result<Func, Error> ?
            return null;
        }

        return matcher.Match;
    }

    private static ObjectMatcher? EmitInternal(EmitContext context)
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

            var matcher = EmitInternal(context.Next());
            if (matcher is null) return null;
            return new Property(context.Token.Span.ToString(), matcher);
        }

        if (context.Head is PrimitivePropertyNode p)
        {
            var next = context.Next();
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

