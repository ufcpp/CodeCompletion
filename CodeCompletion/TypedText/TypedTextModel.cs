using CodeCompletion.Emit;
using CodeCompletion.Syntax;
using CodeCompletion.Text;
using System.Runtime.InteropServices;

namespace CodeCompletion.TypedText;

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
        t.Filter(token.Span, new(_root), results);
    }

    private TypedToken? GetToken(int pos)
    {
        if (pos == 0) return _root;
        return _tokens.ElementAtOrDefault(pos - 1);
    }

    public void Refresh()
    {
        var t = _root;
        var context = new GetCandidatesContext(t);

        _tokens.Clear();
        foreach (var token in Texts.Tokens)
        {
            if (t.Select(token.Span, context) is { } candidate)
            {
                t = candidate.GetToken();
                _tokens.Add(t);
            }
            else
            {
                break;
            }
        }
    }

    public void Reset(ReadOnlySpan<char> source)
    {
        Texts.Reset(source);
        Refresh();
    }

    public Func<object?, bool>? Emit()
    {
        var node = Parser.Parse(Texts);
        var m = Emitter.Emit(node, Root, TokensAsSpan!)!;
        return m.Match;
    }
}
