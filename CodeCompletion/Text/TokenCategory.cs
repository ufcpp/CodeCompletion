namespace CodeCompletion.Text;

public enum TokenCategory
{
    /// <summary>
    /// 不明。
    /// </summary>
    Unknown,

    /// <summary>
    /// 空文字列。
    /// </summary>
    Empty,

    /// <summary>
    /// 空白。
    /// </summary>
    WhiteSpace,

    /// <summary>
    /// 識別子。
    /// </summary>
    /// <remarks>
    /// といいつつ、キーワード(true, false, null 等)もこれ使ってる。
    /// </remarks>
    Identifier,

    /// <summary>
    /// 10進リテラル。
    /// </summary>
    Number,

    // 16進リテラル (Emit 側でちゃんとやってないのでいったんなくす。)
    //HexNumber,

    /// <summary>
    /// 演算子。
    /// といいつつ、equality, comparison のみ。
    /// </summary>
    Operator,

    /// <summary>
    /// ""
    /// </summary>
    String,

    /// <summary>
    /// 組み込み演算みたいなのを . 開始 + ASCII letter にした。
    /// (.length とか .floor とか。)
    /// </summary>
    DotIntrinsics,

    /// <summary>
    /// , とか。
    /// </summary>
    /// <remarks>
    /// 最初は Punctuation って名前で , ( ) を含めてたけど、
    /// regex 演算子足したことで「孤立トークン」(1文字限りのトークン)に変えた。
    /// regex だけ分ける意味もなく。
    ///
    /// <see cref="Operator"/> の方を Comparison とかに返る方がいいかもしれない。
    /// </remarks>
    Isolation,
}
