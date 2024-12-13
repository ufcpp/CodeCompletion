using ObjectMatching.Reflection;

namespace ObjectMatching.Emit;

using Res = Result<ObjectMatcher, BoxedErrorCode>;

internal abstract class ObjectMatcher
{
    public abstract bool Match(object? value);
}

internal abstract class ObjectMatcher<T> : ObjectMatcher
{
    public override bool Match(object? value) => value is T x && Match(x);
    public abstract bool Match(T value);
}

internal class Property(string name, ObjectMatcher mather) : ObjectMatcher
{
    public override bool Match(object? value)
    {
        if (value is null) return false;
        var p = value.GetType().GetProperty(name);
        if (p is null) return false;
        return mather.Match(p.GetValue(value));
    }
}

internal static class Intrinsic
{
    public static Res Create(string name, TypeInfo type, ObjectMatcher matcher)
    {
        if (type.Type == typeof(float)) return FloatIntrinsic<float>.Create(name, matcher);
        if (type.Type == typeof(double)) return FloatIntrinsic<double>.Create(name, matcher);
        if (type.Type == typeof(decimal)) return FloatIntrinsic<decimal>.Create(name, matcher);
        if (name == IntrinsicNames.Length)
        {
            if (type.Type == typeof(string)) return new StringLength(matcher);
            if (type.GetElementType() is { }) return new ArrayLength(matcher);
        }

        return BoxedErrorCode.InvalidIntrinsic;
    }
}
