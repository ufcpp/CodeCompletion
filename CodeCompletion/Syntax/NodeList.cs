using CodeCompletion.Text;

namespace CodeCompletion.Syntax;

public readonly struct NodeList
{
    private readonly TextBuffer _source;
    private readonly Bucket[] _buckets;
    private readonly int[] _indexes;

    internal NodeList(TextBuffer source, Bucket[] buckets, int[] indexes)
    {
        _source = source;
        _buckets = buckets;
        _indexes = indexes;
    }

    public int Length => _indexes.Length;
    public Node this[int index] => new(_source, _buckets, _indexes[index]);

    public Enumerator GetEnumerator() => new(this);

    public struct Enumerator(NodeList list)
    {
        private int _i = -1;
        public bool MoveNext() => ++_i < list.Length;
        public readonly Node Current => list[_i];
    }
}
