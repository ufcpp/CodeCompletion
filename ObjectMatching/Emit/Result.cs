namespace ObjectMatching.Emit;

/// <summary>
/// 例外を throw するんじゃなくて、union で 値 or エラー を返すようにする。
/// </summary>
public readonly struct Result<TValue, TError>
    where TValue : class
    where TError : class
{
    private readonly object _value;

    public Result(TValue value) => _value = value;
    public TValue? Value => _value as TValue;
    public bool IsValue => _value is TValue;
    public static implicit operator Result<TValue, TError>(TValue value) => new(value);

    public Result(TError value) => _value = value;
    public TError? Error => _value as TError;
    public bool IsError => _value is TError;
    public static implicit operator Result<TValue, TError>(TError value) => new(value);
}
