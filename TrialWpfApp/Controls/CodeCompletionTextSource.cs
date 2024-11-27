using CodeCompletion.TypedText;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace TrialWpfApp.Controls;

internal class CodeCompletionTextSource(TypedTextModel semantics, CommonTextProperties textRunProperties, double lineHeight) : TextSource
{
    public int Length => semantics.Texts.TotalLength;
    public int CaretIndex => semantics.Texts.Cursor;

    public GenericTextParagraphProperties ParagraphProperties { get; } = new(lineHeight, textRunProperties);

    public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit)
    {
        var buffer = semantics.Texts;
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
        var buffer = semantics.Texts;

        if (textSourceCharacterIndex > buffer.TotalLength) return _end;

        var (token, pos) = buffer.GetPosition(textSourceCharacterIndex);
        var array = buffer.Tokens[token].ArraySegment;

        if (array.Count == 0) return new TextCharacters(" ", GetTextRunProperties(null));
        if (array.Count == pos) return new TextCharacters(" ", GetTextRunProperties(null));

        var props = GetTextRunProperties(token);
        return new TextCharacters(array.Array, 0, array.Count, props);
    }

    private static readonly TextEndOfParagraph _end = new(1);

    private GenericTextRunProperties GetTextRunProperties(int token)
    {
        var t = semantics.Tokens.ElementAtOrDefault(token);
        return GetTextRunProperties(t);
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

    private static int GetBrushIndex(TypedToken? token) => token switch
    {
        PropertyToken => 1,
        PrimitivePropertyToken => 2,
        CompareToken => 3,
        LiteralToken => 4,
        KeywordToken => 5,
        _ => 0,
    };

    private readonly GenericTextRunProperties?[] _runProps = new GenericTextRunProperties[_tokenBrushes.Length];

    private GenericTextRunProperties GetTextRunProperties(TypedToken? token)
    {
        var i = GetBrushIndex(token);
        return _runProps[i] ?? new(_tokenBrushes[i], textRunProperties);
    }
}
