namespace CodeCompletion.Emit;

// Where(x => x != null) する負荷の方が高そうなので、nullable 受け取って中で無視することに。
internal class And(params ObjectMatcher?[] children) : ObjectMatcher
{
    public override bool Match(object? value)
    {
        foreach (var child in children)
        {
            if (child is null) continue;
            if (!child.Match(value)) return false;
        }
        return true;
    }

    public static ObjectMatcher Create(IReadOnlyList<ObjectMatcher> children)
    {
        if (children is [var single]) return single;
        return new And([.. children]);
    }
}

internal class Or(params ObjectMatcher?[] children) : ObjectMatcher
{
    public override bool Match(object? value)
    {
        foreach (var child in children)
        {
            if (child is null) continue;
            if (child.Match(value)) return true;
        }
        return false;
    }
}
