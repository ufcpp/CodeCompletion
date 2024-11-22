using CodeCompletion.Semantics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace TrialWpfApp;

public class CodeCompletionTextSource(SemanticModel semantics, GenericTextRunProperties textRunProperties) : TextSource
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

        if (array.Count == 0) return new TextEndOfParagraph(1);
        if (array.Count == pos) return new TextCharacters(" ", new RunProp(Brushes.Black, textRunProperties));

        var props = GetTextRunProperties(token);
        return new TextCharacters(array.Array, 0, array.Count, props);
    }

    private RunProp GetTextRunProperties(int token)
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
        var props = new RunProp(brush, textRunProperties);
        return props;
    }
}

public readonly record struct GenericTextRunProperties(
    double FontHintingEmSize,
    double FontRenderingEmSize,
    Typeface Typeface
    );

class RunProp(Brush foregroundBrush, GenericTextRunProperties generalProps) : TextRunProperties
{
    public override Brush ForegroundBrush => foregroundBrush;
    public override double FontHintingEmSize => generalProps.FontHintingEmSize;
    public override double FontRenderingEmSize => generalProps.FontRenderingEmSize;
    public override Typeface Typeface => generalProps.Typeface;

    public override Brush BackgroundBrush => Brushes.White; // 色変えれるようにする？その場合 Fore 側 dark/light 切り替えれないときつい？
    public override CultureInfo CultureInfo => CultureInfo.InvariantCulture;
    public override TextDecorationCollection TextDecorations => [];
    public override TextEffectCollection TextEffects => [];
}

class ParaProp(double height, GenericTextRunProperties generalProps) : TextParagraphProperties
{
    public override TextRunProperties DefaultTextRunProperties => new RunProp(Brushes.Black, generalProps);
    public override bool FirstLineInParagraph => true;
    public override FlowDirection FlowDirection => FlowDirection.LeftToRight;
    public override double Indent => 0;
    public override double LineHeight => height;
    public override TextAlignment TextAlignment => TextAlignment.Left;
    public override TextMarkerProperties TextMarkerProperties => null!;
    public override TextWrapping TextWrapping => TextWrapping.Wrap;
}
