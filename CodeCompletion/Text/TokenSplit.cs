namespace CodeCompletion.Text;

public enum TokenSplit
{
    /// <summary>
    /// Token 分割せずに文字を挿入。
    /// </summary>
    Insert,

    /// <summary>
    /// Token 分割した上で文字は破棄。
    /// </summary>
    Split,

    /// <summary>
    /// 文字を Token に挿入してから Token 分割。
    /// </summary>
    InsertThenSplit,

    /// <summary>
    /// Token 分割してから文字を Token に挿入。
    /// </summary>
    SplitThenInsert,
}
