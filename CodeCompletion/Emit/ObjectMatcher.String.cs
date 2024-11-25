namespace CodeCompletion.Emit;

internal class Length(ObjectMatcher mather) : ObjectMatcher<string>
{
    public override bool Match(string value) => mather.Match(value.Length);
}
