using CodeCompletion.Text;

namespace CodeCompletion.Semantics;

public class SemanticModel
{
    private readonly Node _root;
    private readonly TextBuffer _texts;

    private readonly List<Node?> _nodes = [];

    public SemanticModel(Type rootType, TextBuffer texts)
        : this(Node.Create(rootType), texts) { }

    public SemanticModel(Node root, TextBuffer texts)
    {
        _root = root;
        _texts = texts;
        Refresh();
    }

    public IEnumerable<Node?> Nodes => _nodes;

    public IReadOnlyList<Candidate> GetCandidates()
    {
        var (pos, _) = _texts.GetPosition();
        if (GetNode(pos) is not { } node) return [];

        var token = _texts.Tokens[pos];
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
        foreach (var token in _texts.Tokens)
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
}
