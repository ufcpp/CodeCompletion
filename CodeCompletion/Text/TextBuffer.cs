using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

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
    public ReadOnlySpan<Token> Tokens => CollectionsMarshal.AsSpan(_tokens);

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

    /// <summary>
    /// <paramref name="token"/> 版のトークンを前のトークンと結合する。
    /// </summary>
    private void MergeToken(int token)
    {
        var tokens = Tokens;

        if (token >= tokens.Length) return;

        tokens[token - 1].Insert(tokens[token - 1].Written, tokens[token].Span);
        _tokens.RemoveAt(token);

    }

    /// <summary>
    /// 文字の削除。
    /// </summary>
    public void Remove(CursorMove move)
    {
        var (token, position) = GetPosition();
        var tokens = CollectionsMarshal.AsSpan(_tokens);
        ref var t = ref tokens[token];

        switch (move)
        {
            case CursorMove.Back:
                if (position == 0)
                {
                    if (token == 0) return;
                    MergeToken(token);
                    _cursor--;
                    return;
                }

                t.Remove((position - 1)..position);
                _cursor--;
                break;
            case CursorMove.Forward:
                if (position == t.Span.Length)
                {
                    MergeToken(token + 1);
                    return;
                }

                t.Remove(position..(position + 1));
                break;
            case CursorMove.StartToken:
                if (position == 0)
                {
                    if (token == 0) return;
                    MergeToken(token);
                    _cursor--;
                    return;
                }

                t.Remove(..position);
                _cursor -= position;
                break;
            case CursorMove.EndToken:
                if (position == t.Span.Length)
                {
                    MergeToken(token + 1);
                    return;
                }

                t.Remove(position..);
                break;
        }
    }

    /// <summary>
    /// <see cref="Cursor"/> の位置に文字列挿入。
    /// </summary>
    public void Insert(ReadOnlySpan<char> s)
    {
        var tokens = CollectionsMarshal.AsSpan(_tokens);
        var (token, position) = GetPosition(tokens, _cursor);
        ref var currentToken = ref tokens[token];

        //todo: s.EnumerateRunes でループ回す？
        // IME で長文入力したときだけが関係するんだけど。
        Rune.DecodeFromUtf16(s, out var c, out _);
        var cat = TokenCategorizer.Categorize(currentToken.Span, c);

        switch (cat)
        {
            case TokenSplit.Split:
                split(ref currentToken);
                break;
            case TokenSplit.Insert:
                insert(ref currentToken, s);
                break;
            case TokenSplit.InsertThenSplit:
                insert(ref currentToken, s);
                position += s.Length;
                split(ref currentToken);
                break;
            case TokenSplit.SplitThenInsert:
                split(ref currentToken);
                if (position <= currentToken.Written)
                {
                    position = 0;
                    insert(ref CollectionsMarshal.AsSpan(_tokens)[token + 1], s);
                }
                break;
        }

        void split(ref Token currentToken)
        {
            var newToken = currentToken.Split(position);
            _tokens.Insert(token + 1, newToken);
            _cursor++;
        }

        void insert(ref Token currentToken, ReadOnlySpan<char> s)
        {
            currentToken.Insert(position, s);
            _cursor += s.Length;
        }
    }

    public void Replace(ReadOnlySpan<char> s)
    {
        // 今のトークンを丸ごと置き換え。
        var tokens = CollectionsMarshal.AsSpan(_tokens);
        var (token, _) = GetPosition(tokens, _cursor);
        ref var currentToken = ref tokens[token];
        currentToken.Replace(s);

        // たぶん、後ろに新規トークン挿入した方がストレスなさげ。
        _tokens.Insert(token + 1, new());

        // カーソル位置は末尾(というか、新規挿入したトークンの先頭)に移動。
        _cursor = PositionAtEndOfToken(token);

        int PositionAtEndOfToken(int token)
            => _tokens.Take(token + 1).Sum(x => x.Written) + token + 1;
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
    public (int token, int position) GetPosition() => GetPosition(_cursor);

    public (int token, int position) GetPosition(int cursor)
        => GetPosition(CollectionsMarshal.AsSpan(_tokens), cursor);

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
