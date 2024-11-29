using CodeCompletion.Emit;
using CodeCompletion.Syntax;
using CodeCompletion.Text;
using System.Runtime.InteropServices;

namespace CodeCompletion.TypedText;

/// <summary>
/// コード補完候補を出すために、<see cref="TextBuffer.Tokens"/> に型情報を与えるためのモデル。
/// </summary>
public class TypedTextModel
{
    private readonly TypedToken _root;
    public TextBuffer Texts { get; }

    private readonly List<TypedToken?> _tokens = [];

    public TypedTextModel(Type rootType, TextBuffer? texts = null)
        : this((PropertyToken)TypedToken.Create(rootType), texts) { }

    public TypedTextModel(TypedToken root, TextBuffer? texts = null)
    {
        _root = root;
        Texts = texts ?? new();
        Refresh();
    }

    public TypedToken Root => _root;
    public IEnumerable<TypedToken?> Tokens => _tokens;
    internal ReadOnlySpan<TypedToken?> TokensAsSpan => CollectionsMarshal.AsSpan(_tokens);

    public void GetCandidates(IList<Candidate> results)
    {
        var (pos, _) = Texts.GetPosition();
        if (GetToken(pos) is not { } t) return;

        var token = Texts.Tokens[pos];
        t.Filter(token.Span, _propertyTokens[^1], results);
    }

    private TypedToken? GetToken(int pos)
    {
        if (pos == 0) return _root;
        return _tokens.ElementAtOrDefault(pos - 1);
    }

    public void Refresh()
    {
        var t = _root;

        _tokens.Clear();

        var property = (PropertyTokenBase)_root;

        var stack = new Stack<PropertyTokenBase>();
        stack.Push(property);
        _propertyTokens.Add((new(property, property)));

        var tokens = Texts.Tokens;

        for (int i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];

            if (t.Select(token.Span, _propertyTokens[^1]) is { } candidate)
            {
                t = candidate.GetToken();
                _tokens.Add(t);

                if (t is PropertyTokenBase pt)
                {
                    property = pt;
                }
                else if (t is OpenParenToken)
                {
                    stack.Push(property);
                }
                else if (t is CloseParenToken)
                {
                    stack.Pop();
                }
                _propertyTokens.Add(new(property, stack.Peek()));
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// Item (A = 1, B = 2, Child X (>=1, <= 5)) みたいに書いたとき、
    /// ( の直前の <see cref="PropertyToken"/> を保持しておくためのスタック。
    /// </summary>
    private readonly List<PropertyHierarchy> _propertyTokens = [];

    public void Reset(ReadOnlySpan<char> source)
    {
        Texts.Reset(source);
        Refresh();
    }

    public Func<object?, bool>? Emit()
    {
        var node = Parser.Parse(Texts);
        if (node.IsNull) return null;

        var rootType = ((PropertyToken)_root).Type;
        var m = Emitter.Emit(node, rootType)!;
        if (m is null) return null;
        return m.Match;
    }
}
