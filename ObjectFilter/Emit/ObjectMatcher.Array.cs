using System.Collections;

namespace ObjectFilter.Emit;

internal class ArrayLength(ObjectMatcher mather) : ObjectMatcher
{
    public override bool Match(object? value)
    {
        if (value is Array array) return mather.Match(array.Length);
        if (value is IList list) return mather.Match(list.Count);
        if (value is IEnumerable e) return mather.Match(e.Cast<object>().Count());
        return false;
    }
}

internal class ArrayAny(ObjectMatcher mather) : ObjectMatcher<IEnumerable>
{
    public override bool Match(IEnumerable value)
    {
        foreach (var item in value)
        {
            if (mather.Match(item)) return true;
        }
        return false;
    }
}

internal class ArrayAll(ObjectMatcher mather) : ObjectMatcher<IEnumerable>
{
    public override bool Match(IEnumerable value)
    {
        foreach (var item in value)
        {
            if (!mather.Match(item)) return false;
        }
        return true;
    }
}
