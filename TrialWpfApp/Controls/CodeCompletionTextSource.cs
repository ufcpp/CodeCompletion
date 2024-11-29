using CodeCompletion.Text;
using CodeCompletion.Completion;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace TrialWpfApp.Controls;

internal class CodeCompletionTextSource(TextBuffer texts, CommonTextProperties textRunProperties, double lineHeight) : TextSource
{
    public int Length => texts.TotalLength;
    public int CaretIndex => texts.Cursor;

    public GenericTextParagraphProperties ParagraphProperties { get; } = new(lineHeight, textRunProperties);

    public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit)
    {
        var buffer = texts;
        var (token, pos) = buffer.GetPosition(textSourceCharacterIndexLimit);
        var array = buffer.Tokens[token].ArraySegment;

        var range = new CharacterBufferRange(array.Array, 0, pos);

        return new(pos, new(CultureInfo.InvariantCulture, range));
    }

    public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
    {
        throw new NotImplementedException();
    }

    public override TextRun GetTextRun(int textSourceCharacterIndex)
    {
        var buffer = texts;

        if (textSourceCharacterIndex > buffer.TotalLength) return _end;

        var (token, pos) = buffer.GetPosition(textSourceCharacterIndex);
        var array = buffer.Tokens[token].ArraySegment;

        if (array.Count == 0) return new TextCharacters(" ", GetTextRunProperties(0));
        if (array.Count == pos) return new TextCharacters(" ", GetTextRunProperties(0));

        var props = GetTextRunProperties(buffer.Tokens[token].Span);
        return new TextCharacters(array.Array, 0, array.Count, props);
    }

    private static readonly TextEndOfParagraph _end = new(1);

    private GenericTextRunProperties GetTextRunProperties(int i)
    {
        return _runProps[i] ?? new(_tokenBrushes[i], textRunProperties);
    }

    private GenericTextRunProperties GetTextRunProperties(ReadOnlySpan<char> token)
    {
        var cat = Tokenizer.Categorize(token);

        var i = cat switch
        {
            TokenCategory.Identifier or TokenCategory.DotIntrinsics => 1,
            TokenCategory.Operator => 3,
            TokenCategory.Number or TokenCategory.String => 4,
            _ => 0,
        };

        //todo: 一時的に Keyword, Primitive 出なくなってる。
        return GetTextRunProperties(i);
    }

    private static readonly Brush[] _tokenBrushes =
    [
        Brushes.Black,
        Brushes.MidnightBlue,
        Brushes.DarkGreen,
        Brushes.DimGray,
        Brushes.DarkRed,
        Brushes.Blue,
    ];

    //private static int GetBrushIndex(TypedToken? token) => token switch
    //{
    //    PropertyToken => 1,
    //    PrimitivePropertyToken => 2,
    //    CompareToken => 3,
    //    LiteralToken => 4,
    //    KeywordToken => 5,
    //    _ => 0,
    //};

    private readonly GenericTextRunProperties?[] _runProps = new GenericTextRunProperties[_tokenBrushes.Length];
}
