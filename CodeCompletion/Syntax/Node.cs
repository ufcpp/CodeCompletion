using CodeCompletion.Text;

namespace CodeCompletion.Syntax;

/// <summary>
/// 構文木のノード。
/// </summary>
/// <remarks>
/// アロケーション除けのために構造体にしてるので、ノードの種類によっては不要なフィールド持ってる。
/// </remarks>
public readonly struct Node
{
    private readonly TextBuffer _source;
    private readonly Bucket[] _buckets;
    private readonly int _index;

    internal Node(TextBuffer source, Bucket[] buckets, int index)
    {
        _source = source;
        _buckets = buckets;
        _index = index;
    }

    internal int Index => _index;
    internal Node GetNode(int index) => new(_source, _buckets, index);
    internal ref Bucket GetBucket(int index) => ref _buckets[index];

    /// <summary>
    /// <see cref="NodeType"/>
    /// </summary>
    public NodeType Type => _buckets[_index].Type;

    /// <summary>
    /// <see cref="TextBuffer.Tokens"/> の区間。
    /// </summary>
    public Range Range => _buckets[_index].Span;

    public ReadOnlySpan<Token> Span => _source.Tokens[Range];

    /// <summary>
    /// 第1子。
    /// </summary>
    public Node Left => Create(_buckets[_index].Left);

    /// <summary>
    /// 第2子。
    /// </summary>
    public Node Right => Create(_buckets[_index].Right);

    private Node Create(int index) => index >= 0 ? new(_source, _buckets, index) : default;

    public readonly bool IsNull => _buckets is null;

    /// <summary>
    /// <see cref="Left"/>, <see cref="Right"/> を列挙。
    /// </summary>
    /// <remarks>
    /// (a, b), (c, d) みたいなのは a, b, c, d を列挙したいので、
    /// <see cref="Type"/> が一致してる限り再帰的に子を展開する。
    /// </remarks>
    /// <returns></returns>
    public readonly NodeList GetChildren()
    {
        var list = new List<int>();
        EnumerateChildIndex(list);
        return new(_source, _buckets, [.. list]);
    }

    private void EnumerateChildIndex(List<int> result)
    {
        var l = Left;
        if (l.Type == Type) l.EnumerateChildIndex(result);
        else result.Add(_buckets[_index].Left);

        var r = Right;
        if (r.Type == Type) r.EnumerateChildIndex(result);
        else result.Add(_buckets[_index].Right);
    }
}
