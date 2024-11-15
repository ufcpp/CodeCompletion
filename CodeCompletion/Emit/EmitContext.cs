using CodeCompletion.Semantics;
using CodeCompletion.Text;

namespace CodeCompletion.Emit;

public readonly ref struct EmitContext(Node head, ReadOnlySpan<Node> tail, ReadOnlySpan<Token> tokens)
{
    public readonly Node Head = head;
    public readonly ReadOnlySpan<Node> Tail = tail;
    private readonly ReadOnlySpan<Token> _tokens = tokens;

    public Token Token => _tokens[0];

    public bool IsDefault => Head is null;

    public EmitContext Next()
    {
        if (Tail.Length == 0) return default;

        var nextHead = Tail[0];
        return new(nextHead, Tail[1..], _tokens[1..]);
    }
}

