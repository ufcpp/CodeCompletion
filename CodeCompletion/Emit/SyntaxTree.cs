using CodeCompletion.TypedText;

namespace CodeCompletion.Emit;

/// <summary>
/// A (B=1, C=2) みたいなのを、
/// A
///   B=1
///   C=2
/// みたいなツリー構造にする。
/// </summary>
/// <remarks>
/// 現状 , しかないので単に , での split。
/// | & 実装したら優先度によってツリー化必要だし、() での優先度付けも実装予定。
/// </remarks>
public readonly struct SyntaxTree
{
    private readonly TypedTextModel _semantics;

    // , しかないうちは , のインデックス一覧だけ取っておけば事足りる。
    private readonly List<int> _indexes;

    public SyntaxTree(TypedTextModel semantics)
    {
        _semantics = semantics;

        var indexes = new List<int>();
        var i = 0;
        foreach (var token in semantics.Tokens)
        {
            if (token is CommaToken)
                indexes.Add(i);
            i++;
        }
        indexes.Add(i); // 末尾も入れとくと Emit 側の分岐が楽。
        _indexes = indexes;
    }

    public Func<object?, bool>? Emit()
    {
        var root = _semantics.Root;
        var typedTokens = _semantics.TokensAsSpan;
        var tokens = _semantics.Texts.Tokens;

        var children = new List<ObjectMatcher>();

        var prev = 0;
        foreach (var i in _indexes)
        {
            var c = new EmitContext((PropertyToken)root,
                typedTokens[prev..i]!,
                tokens[prev..i]);

            if (Emitter.Emit(c) is { } m) children.Add(m);

            prev = i + 1;
        }

        return And.Create(children).Match;
    }
}
