using CodeCompletion.Emit;
using CodeCompletion.Text;
using System.Runtime.InteropServices;

namespace CodeCompletion.Semantics;

public class SemanticModel
{
    private readonly Node _root;
    public TextBuffer Texts { get; }

    private readonly List<Node?> _nodes = [];

    public SemanticModel(Type rootType, TextBuffer? texts = null)
        : this((PropertyNode)Node.Create(rootType), texts) { }

    public SemanticModel(Node root, TextBuffer? texts = null)
    {
        _root = root;
        Texts = texts ?? new();
        Refresh();
    }

    public IEnumerable<Node?> Nodes => _nodes;

    public IReadOnlyList<Candidate> GetCandidates()
    {
        var (pos, _) = Texts.GetPosition();
        if (GetNode(pos) is not { } node) return [];

        var token = Texts.Tokens[pos];
        return node.Filter(token.Span);
    }

    private Node? GetNode(int pos)
    {
        if (pos == 0) return _root;
        return _nodes.ElementAtOrDefault(pos - 1);
    }

    public void Refresh()
    {
        var node = _root;

        _nodes.Clear();
        foreach (var token in Texts.Tokens)
        {
            if (node.Select(token.Span) is { } candidate)
            {
                node = candidate.GetNode();
                _nodes.Add(node);
            }
            else
            {
                break;
            }
        }
    }

    public Func<object?, bool>? Emit()
    {
        var c = new EmitContext((PropertyNode)_root,
            CollectionsMarshal.AsSpan(_nodes)!,
            Texts.Tokens);

        return Emitter.Emit(c);
    }
}
