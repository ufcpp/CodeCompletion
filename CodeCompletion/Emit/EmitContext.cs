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

