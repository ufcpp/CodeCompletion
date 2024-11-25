using System.Text.RegularExpressions;

namespace CodeCompletion.Emit;

internal class Length(ObjectMatcher mather) : ObjectMatcher<string>
{
    public override bool Match(string value) => mather.Match(value.Length);
}

internal class RegexMatcher(string pattern) : ObjectMatcher<string>
{
    private readonly Regex _reg = new(pattern);

    public override bool Match(string value) => _reg.Match(value).Success;
}
