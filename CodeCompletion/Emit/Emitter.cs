using CodeCompletion.Semantics;
using CodeCompletion.Text;

namespace CodeCompletion.Emit;

public readonly ref struct EmitContext(Type type, Node head, ReadOnlySpan<Node> tail, ReadOnlySpan<Token> tokens)
{
    internal EmitContext(PropertyNode head, ReadOnlySpan<Node> tail, ReadOnlySpan<Token> tokens)
        : this(head.Type, head, tail, tokens) { }

    public readonly Type Type = type;
    public readonly Node Head = head;
    public readonly ReadOnlySpan<Node> Tail = tail;
    private readonly ReadOnlySpan<Token> _tokens = tokens;

    public Token Token => _tokens[0];

    public bool IsDefault => Head is null;

    public EmitContext Next()
    {
        if (Tail.Length == 0) return default;

        var nextHead = Tail[0];
        var t = Type;
        if (nextHead is PropertyNode p) t = p.Type;
        return new(t, nextHead, Tail[1..], _tokens[1..]);
    }
}

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

