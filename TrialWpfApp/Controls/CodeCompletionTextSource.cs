using CodeCompletion.Semantics;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace TrialWpfApp.Controls;

internal class CodeCompletionTextSource(SemanticModel semantics, CommonTextProperties textRunProperties, double lineHeight) : TextSource
{
    public int Length => semantics.Texts.TotalLength;

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
        var node = semantics.Nodes.ElementAtOrDefault(token);
        return GetTextRunProperties(node);
    }

    private static readonly Brush[] _nodeBrushes =
    [
        Brushes.Black,
        Brushes.MidnightBlue,
        Brushes.DarkGreen,
        Brushes.DimGray,
        Brushes.DarkRed,
        Brushes.Blue,
    ];

    private static int GetBrushIndex(Node? node) => node switch
    {
        PropertyNode => 1,
        PrimitivePropertyNode => 2,
        CompareNode => 3,
        LiteralNode => 4,
        KeywordNode => 5,
        _ => 0,
    };

    private readonly GenericTextRunProperties?[] _runProps = new GenericTextRunProperties[_nodeBrushes.Length];

    private GenericTextRunProperties GetTextRunProperties(Node? node)
    {
        var i = GetBrushIndex(node);
        return _runProps[i] ?? new(_nodeBrushes[i], textRunProperties);
    }
}
