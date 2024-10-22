namespace CodeCompletion.Text;

/// <summary>
/// カーソル移動方法。
/// </summary>
public enum CursorMove
{
    /// <summary>
    /// 1文字戻る。
    /// </summary>
    Back = 1,

    /// <summary>
    /// 1文字進む。
    /// </summary>
    Forward,

    /// <summary>
    /// トークンの先頭に移動。
    /// </summary>
    StartToken,

    /// <summary>
    /// トークンの末尾に移動。
    /// </summary>
    EndToken,

    /// <summary>
    /// 先頭に移動。
    /// </summary>
    StartText,

    /// <summary>
    /// 末尾に移動。
    /// </summary>
    EndText,
}
