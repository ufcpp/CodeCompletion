namespace CodeCompletion.Syntax;

/// <summary>
/// <see cref="Node"/> の中身。
/// アロケーション除けのために、参照の代わりに配列のインデックスでデータを持ってる。
/// </summary>
/// <param name="Span">ソースコード中の区間。</param>
/// <param name="Type"><see cref="NodeType"/></param>
/// <param name="Left">第1子のインデックス。</param>
/// <param name="Right">第2子のインデックス。</param>
internal readonly record struct Bucket(Span Span, NodeType Type, int Left = -1, int Right = -1)
{
    public readonly override string ToString() => $"{Type} {Span.Start}-{Span.End} {Left} {Right}";
}
