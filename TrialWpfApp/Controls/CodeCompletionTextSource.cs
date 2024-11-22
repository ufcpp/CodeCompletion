using CodeCompletion.Semantics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace TrialWpfApp.Controls;

internal class CodeCompletionTextSource(SemanticModel semantics, CommonTextProperties textRunProperties) : TextSource
{
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

        if (textSourceCharacterIndex > buffer.TotalLength) return new TextEndOfParagraph(1);

        var (token, pos) = buffer.GetPosition(textSourceCharacterIndex);
        var array = buffer.Tokens[token].ArraySegment;

        if (array.Count == 0) return new TextCharacters(" ", new GenericTextRunProperties(Brushes.Black, textRunProperties)); ;
        if (array.Count == pos) return new TextCharacters(" ", new GenericTextRunProperties(Brushes.Black, textRunProperties));

        var props = GetTextRunProperties(token);
        return new TextCharacters(array.Array, 0, array.Count, props);
    }

    private GenericTextRunProperties GetTextRunProperties(int token)
    {
        var node = semantics.Nodes.ElementAtOrDefault(token);
        var brush = node switch
        {
            PropertyNode => Brushes.MidnightBlue,
            PrimitivePropertyNode => Brushes.DarkGreen,
            CompareNode => Brushes.DimGray,
            LiteralNode => Brushes.DarkRed,
            KeywordNode => Brushes.Blue,
            _ => Brushes.Black,
        };
        var props = new GenericTextRunProperties(brush, textRunProperties);
        return props;
    }
}
