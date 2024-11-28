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

    /// <summary>
    /// comma_expression
    ///   | or_expression
    ///   | or_expression ',' comma_expression
    /// </summary>
    private static (int end, int nodeIndex) CommaExpression(ReadOnlySpan<Token> span, int start, Builder builder)
    {
        var (end, firstIndex) = OrExpression(span, start, builder);
        if (end == span.Length || span[end].Span is not [',', ..]) return (end, firstIndex);
        (end, var secondIndex) = CommaExpression(span, end + 1, builder);
        var i = builder.New(new(start, end), NodeType.Comma, firstIndex, secondIndex);
        return (end, i);
    }

    /// <summary>
    /// or_expression
    ///   | and_expression
    ///   | and_expression '|' or_expression
    /// </summary>
    private static (int end, int nodeIndex) OrExpression(ReadOnlySpan<Token> span, int start, Builder builder)
    {
        var (end, firstIndex) = AndExpression(span, start, builder);
        if (end == span.Length || span[end].Span is not ['|', ..]) return (end, firstIndex);
        (end, var secondIndex) = OrExpression(span, end + 1, builder);
        var i = builder.New(new(start, end), NodeType.Or, firstIndex, secondIndex);
        return (end, i);
    }

    /// <summary>
    /// and_expression
    ///   | primary_expression
    ///   | primary_expression '&' and_expression
    /// </summary>
    private static (int end, int nodeIndex) AndExpression(ReadOnlySpan<Token> span, int start, Builder builder)
    {
        var (end, firstIndex) = TermExpression(span, start, builder);
        if (end == span.Length || span[end].Span is not ['&', ..]) return (end, firstIndex);
        (end, var secondIndex) = AndExpression(span, end + 1, builder);
        var i = builder.New(new(start, end), NodeType.And, firstIndex, secondIndex);
        return (end, i);
    }

    /// <summary>
    /// term_expression
    ///   | '(' comma_expression ')'
    ///   | primary_expression
    /// </summary>
    private static (int end, int nodeIndex) TermExpression(ReadOnlySpan<Token> span, int start, Builder builder)
    {
        if (span[start].Span is ['(', ..])
        {
            return ParenthesizedExpression(span, start, builder);
        }
        else
        {
            return MemberAccessExpression(span, start, builder);
        }
    }

    private static (int end, int nodeIndex) ParenthesizedExpression(ReadOnlySpan<Token> span, int start, Builder builder)
    {
        // (comma_expr)
        var (end, i) = CommaExpression(span, start + 1, builder);
        if (end < span.Length && span[end].Span[0] == ')') end++;
        // () 用の Node を作らず中身をそのまま返してるので、Span は1マスずつ内側にずれる。
        return (end, i);
    }

    /// <summary>
    /// primary_expression
    ///   | member_access_expression operator value
    ///   | member_access_expression '(' comma_expression ')'
    ///
    /// member_access_expression
    ///   | identifier
    ///   | identifier member_access_expression
    /// </summary>
    private static (int end, int nodeIndex) MemberAccessExpression(ReadOnlySpan<Token> span, int start, Builder builder)
    {
        var cat = Tokenizer.Categorize(span[start].Span);
        if (cat is not (TokenCategory.Identifier or TokenCategory.DotIntrinsics))
        {
            if (span[start].Span is "(")
                return ParenthesizedExpression(span, start, builder);

            return Comparison(span, start, builder);
        }

        var (end, nextIndex) = MemberAccessExpression(span, start + 1, builder);

        var i = builder.New(new(start, end), NodeType.Member, nextIndex, -1);
        return (end, i);
    }

    private static (int end, int nodeIndex) Comparison(ReadOnlySpan<Token> span, int start, Builder builder)
    {
        if (start + 1 >= span.Length) goto ERROR;

        var opSpan = span[start].Span;
        var valueSpan = span[start + 1].Span;

        var op = opSpan switch
        {
            "=" => NodeType.Equal,
            "!=" => NodeType.NotEqual,
            "<" => NodeType.LessThan,
            "<=" => NodeType.LessThanOrEqual,
            ">" => NodeType.GreaterThan,
            ">=" => NodeType.GreaterThanOrEqual,
            "~" => NodeType.Regex,
            _ => NodeType.Error,
        };

        if (op == NodeType.Error) goto ERROR;

        if (Tokenizer.Categorize(valueSpan) is
            not (TokenCategory.Identifier or TokenCategory.Number or TokenCategory.String))
            goto ERROR;

        var compIndex = builder.New(new(start, start + 2), op, -1, -1);
        return (start + 2, compIndex);

    ERROR:
        return (span.Length, -1); // 例外にする？
    }
}
