using CodeCompletion.TypedText;
using CodeCompletion.Text;

namespace CodeCompletion.Emit;

public readonly ref struct EmitContext(TypedToken head, ReadOnlySpan<TypedToken> tail, ReadOnlySpan<Token> tokens)
{
    public readonly TypedToken Head = head;
    public readonly ReadOnlySpan<TypedToken> Tail = tail;
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

