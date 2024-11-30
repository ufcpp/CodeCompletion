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
    /// 組み込み演算みたいなのを . 開始 + ASCII letter にした。
    /// (.length とか .floor とか。)
    /// </summary>
    DotIntrinsics,

    /// <summary>
    /// 10進リテラル。
    /// </summary>
    Number,

    // 16進リテラル (Emit 側でちゃんとやってないのでいったんなくす。)
    //HexNumber,

    /// <summary>
    /// ""
    /// </summary>
    String,

    /// <summary>
    /// 演算子。
    /// といいつつ、equality, comparison のみ。
    /// </summary>
    Comparison,

    /// <summary>
    /// , | &amp;
    /// </summary>
    Conjunction,

    /// <summary>
    /// ()
    /// </summary>
    /// <remarks>
    /// tokenize ルール的には <see cref="Conjunction"/> と全く一緒。
    /// 色を変えるように enum 値分けた。
    /// </remarks>
    Punctuation,
}
