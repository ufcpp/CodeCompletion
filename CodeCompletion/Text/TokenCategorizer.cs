using System.Globalization;
using System.Text;

namespace CodeCompletion.Text;

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

        //if (c == '0')
        //{
        //    if (text is [_, 'x' or 'X', ..]) return TokenCategory.HexNumber;
        //    else return TokenCategory.Number;
        //}

        return c switch
        {
            >= '0' and <= '9' => TokenCategory.Number,
            //>= '1' and <= '9' => TokenCategory.Number,
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
                //if (text is ['0'] && c.Value is 'x' or 'X') return TokenSplit.Insert;
                return split(c);
            //case TokenCategory.HexNumber:
            //    if (c.Value is (>= '0' and <= '9') or (>= 'a' and <= 'z') or (>= 'A' and <= 'Z')) return TokenSplit.Insert;
            //    return split(c);
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
