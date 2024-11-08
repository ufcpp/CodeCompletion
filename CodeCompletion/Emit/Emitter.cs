using CodeCompletion.Semantics;
using CodeCompletion.Text;

namespace CodeCompletion.Emit;

public readonly ref struct EmitContext(Node head, ReadOnlySpan<Node> tail, ReadOnlySpan<Token> tokens)
{
    public readonly Node Head = head;
    public readonly ReadOnlySpan<Node> Tail = tail;
    private readonly ReadOnlySpan<Token> _tokens = tokens;

    public Token Token => _tokens[0];

    public EmitContext Next() => new(Tail[0], Tail[1..], _tokens[1..]);
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
    public static Func<object?, bool> Emit(EmitContext context)
    {
        var matcher = EmitInternal(context);

        if (matcher is null)
        {
            // 例外？
            // Result<Func, Error> ?
            return null!;
        }

        return matcher.Match;
    }

    private static ObjectMatcher? EmitInternal(EmitContext context)
    {
        if (context.Head is PropertyNode)
        {
            var matcher = EmitInternal(context.Next());
            if (matcher is null) return null;
            return new Property(context.Token.Span.ToString(), matcher);
        }

        if (context.Head is CompareNode)
        {
            //var next = context.Next();
            //if (next.Head is not LiteralNode) return null;

            //return Compare.Create();
        }

        return null;
    }
}

