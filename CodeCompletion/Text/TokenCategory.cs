﻿using System.Globalization;
using System.Text;

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
    Identifier,

    /// <summary>
    /// 10進リテラル。
    /// </summary>
    Number,

    /// <summary>
    /// 16進リテラル。
    /// </summary>
    HexNumber,

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

public static class TokenCategorizer
{
    public static TokenCategory Categorize(ReadOnlySpan<char> text)
    {
        if (text.IsEmpty) return TokenCategory.Empty;

        var c = text[0];
        var uc = char.GetUnicodeCategory(c);

        if (uc == UnicodeCategory.SpaceSeparator
            || c == '\t' || c == '\v' || c == '\f') return TokenCategory.WhiteSpace;

        if ((uint)uc <= 4 // UppercaseLetter, LowercaseLetter, TitlecaseLetter, ModifierLetter, OtherLetter
            || uc == UnicodeCategory.LetterNumber
            || c == '_') return TokenCategory.Identifier;

        if (c == '0')
        {
            if (text is [_, 'x' or 'X', ..]) return TokenCategory.HexNumber;
            else return TokenCategory.Number;
        }

        return c switch
        {
            >= '1' and <= '9' => TokenCategory.Number,
            '<' or '>' or '=' or '!' => TokenCategory.Operator,
            '"' or '\'' => TokenCategory.String,
            '.' => TokenCategory.DotIntrinsics,
            ',' or '(' or ')' or '~' => TokenCategory.Isolation,
            _ => TokenCategory.Unknown
        };
    }

    public static TokenSplit Categorize(ReadOnlySpan<char> text, Rune c)
    {
        var uc = Rune.GetUnicodeCategory(c);

        static bool isWhiteSpace(Rune c)
            => Rune.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator
            || c.Value == '\t' || c.Value == '\v' || c.Value == '\f';

        static TokenSplit split(Rune c)
            => isWhiteSpace(c) ? TokenSplit.Split : TokenSplit.SplitThenInsert;

        switch (Categorize(text))
        {
            case TokenCategory.Empty:
                return TokenSplit.Insert;
            case TokenCategory.WhiteSpace:
                if (isWhiteSpace(c)) return TokenSplit.Insert;
                else return TokenSplit.SplitThenInsert;
            case TokenCategory.Identifier:
                if ((uint)uc <= 10 // Letter, Mark, Number
                    || c.Value == '_') return TokenSplit.Insert;
                return split(c);
            case TokenCategory.Number:
                if (c.Value is (>= '0' and <= '9') or '.') return TokenSplit.Insert;
                if (text is ['0'] && c.Value is 'x' or 'X') return TokenSplit.Insert;
                return split(c);
            case TokenCategory.HexNumber:
                if (c.Value is (>= '0' and <= '9') or (>= 'a' and <= 'z') or (>= 'A' and <= 'Z')) return TokenSplit.Insert;
                return split(c);
            case TokenCategory.Operator:
                if (c.Value is '<' or '>' or '=' or '!') return TokenSplit.Insert;
                return split(c);
            case TokenCategory.String:
                if (c.Value is '"' or '\'') return TokenSplit.InsertThenSplit;
                //todo: \" とかの escape 判定
                return TokenSplit.Insert;
            case TokenCategory.DotIntrinsics:
                if (c.IsAscii && (uint)uc <= 4) return TokenSplit.Insert;
                return split(c);
            case TokenCategory.Isolation:
                return split(c);
        }

        if (isWhiteSpace(c)) return TokenSplit.Split;
        else return TokenSplit.Insert;
    }
}
