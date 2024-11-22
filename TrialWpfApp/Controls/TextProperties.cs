using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace TrialWpfApp.Controls;

public readonly record struct CommonTextProperties(
    double FontHintingEmSize,
    double FontRenderingEmSize,
    Typeface Typeface
    )
{
    public CommonTextProperties(double size, FontFamily family, FontStyle style, FontWeight weight, FontStretch stretch)
        : this(size, size, new(family, style, weight, stretch)) { }
}

internal class GenericTextRunProperties(Brush foregroundBrush, CommonTextProperties generalProps) : TextRunProperties
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

internal class GenericTextParagraphProperties(double height, CommonTextProperties generalProps) : TextParagraphProperties
{
    public override TextRunProperties DefaultTextRunProperties => new GenericTextRunProperties(Brushes.Black, generalProps);
    public override bool FirstLineInParagraph => true;
    public override FlowDirection FlowDirection => FlowDirection.LeftToRight;
    public override double Indent => 0;
    public override double LineHeight => height;
    public override TextAlignment TextAlignment => TextAlignment.Left;
    public override TextMarkerProperties TextMarkerProperties => null!;
    public override TextWrapping TextWrapping => TextWrapping.Wrap;
}
