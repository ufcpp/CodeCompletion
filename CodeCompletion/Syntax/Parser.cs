using CodeCompletion.Text;

namespace CodeCompletion.Syntax;

public class Parser
{
    public static Node Parse(TextBuffer source)
    {
        var builder = new Builder();

        var (_, root) = CommaExpression(source.Tokens, 0, builder);

        return new Node(source, builder.GetBuckets(), root);
    }

    private static (int end, int nodeIndex) CommaExpression(ReadOnlySpan<Token> span, int start, Builder builder)
    {
        var (end, firstIndex) = OrExpression(span, start, builder);
        if (end == span.Length || span[end].Span is not [',', ..]) return (end, firstIndex);
        (end, var secondIndex) = CommaExpression(span, end + 1, builder);
        var i = builder.New(new(start, end), NodeType.Comma, firstIndex, secondIndex);
        return (end, i);
    }

    private static (int end, int nodeIndex) OrExpression(ReadOnlySpan<Token> span, int start, Builder builder)
    {
        var (end, firstIndex) = AndExpression(span, start, builder);
        if (end == span.Length || span[end].Span is not ['|', ..]) return (end, firstIndex);
        (end, var secondIndex) = OrExpression(span, end + 1, builder);
        var i = builder.New(new(start, end), NodeType.Or, firstIndex, secondIndex);
        return (end, i);
    }

    private static (int end, int nodeIndex) AndExpression(ReadOnlySpan<Token> span, int start, Builder builder)
    {
        var (end, firstIndex) = PrimaryExpression(span, start, builder);
        if (end == span.Length || span[end].Span is not ['&', ..]) return (end, firstIndex);
        (end, var secondIndex) = AndExpression(span, end + 1, builder);
        var i = builder.New(new(start, end), NodeType.And, firstIndex, secondIndex);
        return (end, i);
    }

    private static (int end, int nodeIndex) PrimaryExpression(ReadOnlySpan<Token> span, int start, Builder builder)
    {
        int x = IndexOfAny(span[start..]);

        if (x >= 0 && span[start + x].Span is ['(', ..])
        {
            // (comma_expr)
            var (end, i) = CommaExpression(span, start + 1, builder);
            if (end < span.Length && span[end].Span[0] == ')') end++;
            // () 用の Node を作らず中身をそのまま返してるので、Span は1マスずつ内側にずれる。
            return (end, i);
        }
        else
        {
            // term
            var end = x < 0 ? span.Length : start + x;
            var i = builder.New((Span)new(start, end));
            return (end, i);

            //todo:
            // いったん A B C = 1 みたいなやつはこの区間をまとめて1トークンとして返してる。
            // せっかく Parse してるんだし、 Property(A, Proparty(B, Compare(C, Literal(1)))) みたいなノード作った方が後が楽かも。
        }

        //todo: term(comma_expr) 対応

        static int IndexOfAny(ReadOnlySpan<Token> span)
        {
            var i = 0;
            foreach (var token in span)
            {
                if (token.Span is [',' or '|' or '&' or '(' or ')'] or []) return i;
                ++i;
            }
            return -1;
        }
    }
}
