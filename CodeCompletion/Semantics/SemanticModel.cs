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

    public Node Root => _root;
    public IEnumerable<Node?> Nodes => _nodes;
    internal ReadOnlySpan<Node?> NodesAsSpan => CollectionsMarshal.AsSpan(_nodes);

    public void GetCandidates(IList<Candidate> results)
    {
        var (pos, _) = Texts.GetPosition();
        if (GetNode(pos) is not { } node) return;

        var token = Texts.Tokens[pos];
        node.Filter(token.Span, new(_root), results);
    }

    private Node? GetNode(int pos)
    {
        if (pos == 0) return _root;
        return _nodes.ElementAtOrDefault(pos - 1);
    }

    public void Refresh()
    {
        var node = _root;
        var context = new GetCandidatesContext(node);

        _nodes.Clear();
        foreach (var token in Texts.Tokens)
        {
            if (node.Select(token.Span, context) is { } candidate)
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

    public void Reset(ReadOnlySpan<char> source)
    {
        Texts.Reset(source);
        Refresh();
    }

    public Func<object?, bool>? Emit()
    {
        var tree = new SyntaxTree(this);
        return tree.Emit();
    }
}
