using CodeCompletion.Text;
using ObjectMatching.Syntax;

namespace ObjectMatching.Emit;

/// <summary>
/// エラー発生個所の情報 + エラー内容。
/// </summary>
public record Error(Range Span, string Token, ErrorCode Code)
{
    /// <summary>
    /// <see cref="Node"/> 情報取れないようなエラー。
    /// </summary>
    public static readonly Error UnknownSyntaxError = new(default, "", ErrorCode.UnknownSyntaxError);
}

public enum ErrorCode
{
    None,
    UnknownSyntaxError,
    PropertyNotFound,
    InvalidIntrinsic,
    InvalidOperator,
    InvalidOperand,
    UnsupportedType,
    InvalidRegex,
}

/// <summary>
/// <see cref="Result{TValue, TError}"/> 都合で値型そのままを使えないので…
/// </summary>
internal record BoxedErrorCode(ErrorCode Code)
{
    public static readonly BoxedErrorCode PropertyNotFound = new(ErrorCode.PropertyNotFound);
    public static readonly BoxedErrorCode InvalidIntrinsic = new(ErrorCode.InvalidIntrinsic);
    public static readonly BoxedErrorCode InvalidOperator = new(ErrorCode.InvalidOperator);
    public static readonly BoxedErrorCode InvalidOperand = new(ErrorCode.InvalidOperand);
    public static readonly BoxedErrorCode UnsupportedType = new(ErrorCode.UnsupportedType);
    public static readonly BoxedErrorCode InvalidRegex = new(ErrorCode.InvalidRegex);
}

internal static class ErrorExtensions
{
    /// <summary>
    /// <see cref="Node"/> から、ソース中のどの区間で、どういうトークンの時にエラーになったかの情報を足す。
    /// </summary>
    public static Result<TValue, Error> With<TValue>(this Result<TValue, BoxedErrorCode> result, Node node)
        where TValue : class
    {
        if (result.Value is { } v) return v;
        return result.Error!.With(node);
    }

    /// <summary>
    /// <see cref="Node"/> から、ソース中のどの区間で、どういうトークンの時にエラーになったかの情報を足す。
    /// </summary>
    public static Error With(this BoxedErrorCode e, Node node)
        => new(node.Range, ToString(node.Span), e.Code);

    // Tokens には X(>100, <200) みたいな区間が入ってることがあって、
    // ErrorCode によって真の原因がどこかって違うので、ErrorCode 見て分岐した方がいいかも。
    private static string ToString(ReadOnlySpan<Token> tokens)
        => tokens is [var t, ..] ? t.Span.ToString() : "";
}
