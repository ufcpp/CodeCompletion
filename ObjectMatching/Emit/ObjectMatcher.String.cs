using System.Text.RegularExpressions;

namespace ObjectMatching.Emit;

using Res = Result<ObjectMatcher, BoxedErrorCode>;

internal class StringLength(ObjectMatcher mather) : ObjectMatcher<string>
{
    public override bool Match(string value) => mather.Match(value.Length);
}

internal class RegexMatcher(Regex pattern) : ObjectMatcher<string>
{
    private readonly Regex _reg = pattern;

    public override bool Match(string value) => _reg.Match(value).Success;

    public static Res Create(ReadOnlySpan<char> escapedPattern)
    {
        var pattern = StringHelper.Unescape(escapedPattern).ToString();

        try
        {
            var regex = new Regex(pattern);
            return new RegexMatcher(regex);
        }
        catch (Exception)
        {
            return BoxedErrorCode.InvalidRegex;
        }
    }
}

internal static class StringHelper
{
    public static ReadOnlySpan<char> Unescape(this ReadOnlySpan<char> span)
    {
        if (span.Length == 0) return span;

        var start = span[0];

        if (start is '\"' or '\'')
        {
            var end = span[^1];
            if (end == start)
            {
                span = span[1..^1];
            }

            //todo: \" の unescape とか(仕様から要検討)。
            // 今は、「" を使いたければ ' 開始にすればいい」的な割り切り。
        }

        return span;
    }
}