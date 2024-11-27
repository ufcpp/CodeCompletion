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
            // (comma_expr)
            var (end, i) = CommaExpression(span, start + 1, builder);
            if (end < span.Length && span[end].Span[0] == ')') end++;
            // () 用の Node を作らず中身をそのまま返してるので、Span は1マスずつ内側にずれる。
            return (end, i);
        }
        else
        {
            return PrimaryExpression(span, start, builder);
        }
    }

    /// <summary>
    /// primary_expression
    ///   | member_access_expression operator value
    ///   todo: | member_access_expression '(' comma_expression ')'
    /// </summary>
    private static (int end, int nodeIndex) PrimaryExpression(ReadOnlySpan<Token> span, int start, Builder builder)
    {
        var (end, memberIndex) = MemberAccessExpression(span, start, builder);

        if (end + 1 >= span.Length) return (end, -1); // 例外にする？

        var opSpan = span[end].Span;
        var valueSpan = span[end + 1].Span;

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

        if (op == NodeType.Error) return (end, -1);

        if (Tokenizer.Categorize(valueSpan) is
            not (TokenCategory.Identifier or TokenCategory.Number or TokenCategory.String))
            return (end, -1);

        var valueIndex = builder.New(new(end + 1, end + 2), NodeType.Value, -1, -1);
        var compIndex = builder.New(new(end, end + 1), op, memberIndex, valueIndex);
        return (end + 2, compIndex);
    }

    /// <summary>
    /// member_access_expression
    ///   | identifier
    ///   | identifier member_access_expression
    /// </summary>
    private static (int end, int nodeIndex) MemberAccessExpression(ReadOnlySpan<Token> span, int start, Builder builder)
    {
        var cat = Tokenizer.Categorize(span[start].Span);
        if (cat is not (TokenCategory.Identifier or TokenCategory.DotIntrinsics)) return (start, -1);

        var (end, nextIndex) = MemberAccessExpression(span, start + 1, builder);

        var i = builder.New(new(start, end), NodeType.Member, nextIndex, -1);
        return (end, i);
    }
}
