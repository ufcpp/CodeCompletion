namespace ObjectMatching.Syntax;

/// <summary>
/// テキスト中の範囲。
/// </summary>
/// <remarks>
/// 64k 超えるテキスト使わないと思うんでメモリケチる。
/// </remarks>
internal record struct Span(ushort Start, ushort End)
{
    public Span(int start, int end) : this((ushort)start, (ushort)end) { }

    public static implicit operator Range(Span span) => new(span.Start, span.End);
}
