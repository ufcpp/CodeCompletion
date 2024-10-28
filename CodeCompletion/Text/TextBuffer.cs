using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CodeCompletion.Text;

/// <summary>
/// テキスト管理用のバッファー。
/// </summary>
/// <remarks>
/// トークン単位で char[] を持つ。
/// </remarks>
public class TextBuffer : ISpanFormattable
{
    private int _cursor;

    private readonly List<Token> _tokens = [new Token()];

    /// <summary>
    /// トークン一覧。
    /// </summary>
    /// <remarks>
    /// Add/Remove は外からされたくないけど、インデックスアクセスでの書き換えは認めててちょっと中途半端な感じあり。
    /// </remarks>
    internal Span<Token> Tokens => CollectionsMarshal.AsSpan(_tokens);

    /// <summary>
    /// テキスト全体の文字数。
    /// </summary>
    /// <remarks>
    /// 「トークンとトークンの間に1スペース挟まってる」換算。
    /// </remarks>
    public int TotalLength => _tokens.Sum(x => x.Written) + _tokens.Count - 1;

    /// <summary>
    /// 現在のカーソル位置。
    /// </summary>
    public int Cursor => _cursor;

    /// <summary>
    /// カーソル移動。
    /// </summary>
    public void Move(CursorMove move)
    {
        switch (move)
        {
            case CursorMove.Back:
                if (_cursor == 0) return;
                _cursor--;
                break;
            case CursorMove.Forward:
                if (_cursor >= TotalLength) return;
                _cursor++;
                break;
            case CursorMove.StartText:
                _cursor = 0;
                break;
            case CursorMove.EndText:
                _cursor = TotalLength;
                break;
            case CursorMove.StartToken:
                {
                    var (_, position) = GetPosition();
                    if (position == 0)
                    {
                        goto case CursorMove.Back;
                    }
                    else
                    {
                        _cursor -= position;
                    }
                }
                break;
            case CursorMove.EndToken:
                {
                    var (token, position) = GetPosition();
                    var written = _tokens[token].Written;
                    if (position == written)
                    {
                        goto case CursorMove.Forward;
                    }
                    else
                    {
                        _cursor -= position;
                        _cursor += written;
                    }
                }
                break;
            default:
                break;
        }
    }

    //todo: backspace
    //  トークン先頭で BS はトークンのマージ
    //  ctlr + BS
    //todo: delete
    //  トークン末尾で DEL はトークンのマージ
    //  ctlr + DEL

    /// <summary>
    /// <see cref="Cursor"/> の位置に文字列挿入。
    /// </summary>
    public void Insert(ReadOnlySpan<char> s)
    {
        if (s.IsWhiteSpace())
        {
            //todo: 挿入はトークン末尾にいるときだけ。
            // 途中にいるときは Split する。
            NewToken();
            return;
        }

        var tokens = CollectionsMarshal.AsSpan(_tokens);
        var (token, position) = GetPosition(tokens, _cursor);

        if (token >= tokens.Length) return;

        ref var currentToken = ref tokens[token];

        if (position > currentToken.Written) return;

        currentToken.Insert(position, s);
        _cursor += s.Length;
    }

    /// <summary>
    /// <see cref="Cursor"/> の位置のトークン文字列を取得。
    /// </summary>
    public ReadOnlySpan<char> GetCurrentToken()
    {
        var tokens = CollectionsMarshal.AsSpan(_tokens);
        var (token, _) = GetPosition(tokens, _cursor);

        if (token >= tokens.Length) return default;

        ref var currentToken = ref tokens[token];

        return currentToken.Span;
    }

    /// <summary>
    /// <see cref="Cursor"/> 位置が何トークン目の何文字目かを取得。
    /// </summary>
    public (int token, int position) GetPosition()
        => GetPosition(CollectionsMarshal.AsSpan(_tokens), _cursor);

    private static (int token, int position) GetPosition(Span<Token> tokens, int cursor)
    {
        int pos = 0;
        var index = 0;
        foreach (ref var t in tokens)
        {
            pos = cursor;
            cursor -= t.Written;
            if (cursor <= 0) break;
            index++;
            cursor--;
        }

        return (index, pos);
    }

    private void NewToken()
    {
        var (token, _) = GetPosition(CollectionsMarshal.AsSpan(_tokens), _cursor);
        _tokens.Insert(token + 1, new Token());
        _cursor++;
    }

    public override string ToString() => ToString(null, null);

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        charsWritten = 0;
        bool part = false;
        foreach (var token in _tokens)
        {
            if (!part) part = true;
            else
            {
                if (destination.Length == 0) return false;
                destination[0] = ' ';
                destination = destination[1..];
                charsWritten++;
            }

            if (token.Written > destination.Length) return false;
            token.Span.CopyTo(destination);
            destination = destination[token.Written..];
            charsWritten += token.Written;
        }
        return true;
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        var s = new DefaultInterpolatedStringHandler(_tokens.Sum(x => x.Written + 1), 0);
        bool part = false;
        foreach (var token in _tokens)
        {
            if (part) s.AppendFormatted(' ');
            else part = true;
            s.AppendFormatted(token.Span);
        }
        return s.ToStringAndClear();
    }
}
