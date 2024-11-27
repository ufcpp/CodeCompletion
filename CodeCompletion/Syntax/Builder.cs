using System.Runtime.InteropServices;

namespace CodeCompletion.Syntax;

internal readonly struct Builder()
{
    private readonly List<Bucket> _buckets = [];
    public ref Bucket this[int index] => ref CollectionsMarshal.AsSpan(_buckets)[index];

    public int New(Bucket x)
    {
        var i = _buckets.Count;
        _buckets.Add(x);
        return i;
    }

    public int New(Span span, NodeType type, int firstIndex, int secondIndex)
        => New(new Bucket(span, type, firstIndex, secondIndex));

    public Bucket[] GetBuckets() => [.. _buckets];
}
