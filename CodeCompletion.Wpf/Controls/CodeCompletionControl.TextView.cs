using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace CodeCompletion.Controls;

partial class CodeCompletionControl
{
    private class TextView(CodeCompletionControl parent) : UIElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (parent.TextSource is not { } textSource) return;

            if (textSource.Length == 0)
            {
                parent.UpdateCaret(0);
                return;
            }

            var formatter = TextFormatter.Create();
            var para = textSource.ParagraphProperties;
            var linePosition = new Point(0, 0);

            int textStorePosition = 0;
            while (textStorePosition < textSource.Length)
            {
                using var line = formatter.FormatLine(
                    textSource,
                    textStorePosition,
                    96 * 6,
                    para,
                    null);

                line.Draw(drawingContext, linePosition, InvertAxes.None);

                var prev = textStorePosition;
                textStorePosition += line.Length;

                var caret = textSource.CaretIndex;
                if (prev <= caret && caret < textStorePosition)
                {
                    var x = line.GetTextBounds(caret, 1)[0].Rectangle.Left;
                    parent.UpdateCaret(x);
                }

                linePosition.Y += line.Height;
            }
        }
    }
}
