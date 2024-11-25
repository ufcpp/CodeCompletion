﻿using CodeCompletion.Semantics;

namespace CodeCompletion.Emit;

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
    public static ObjectMatcher? Create(string name, Type type, ObjectMatcher matcher)
    {
        if (type == typeof(float)) return FloatIntrinsic<float>.Create(name, matcher);
        if (type == typeof(double)) return FloatIntrinsic<double>.Create(name, matcher);
        if (type == typeof(decimal)) return FloatIntrinsic<decimal>.Create(name, matcher);
        if (type == typeof(string) && name == IntrinsicNames.Length) return new Length(matcher);
        return null;
    }
}
