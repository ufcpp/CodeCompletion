namespace CodeCompletion.Emit;

internal class ArrayLength(ObjectMatcher mather) : ObjectMatcher<Array>
{
    public override bool Match(Array value) => mather.Match(value.Length);
}

internal class ArrayAny(ObjectMatcher mather) : ObjectMatcher<Array>
{
    public override bool Match(Array value)
    {
        foreach (var item in value)
        {
            if (mather.Match(item)) return true;
        }
        return false;
    }
}

internal class ArrayAll(ObjectMatcher mather) : ObjectMatcher<Array>
{
    public override bool Match(Array value)
    {
        foreach (var item in value)
        {
            if (!mather.Match(item)) return false;
        }
        return true;
    }
}
