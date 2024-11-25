using CodeCompletion.Semantics;
using System.Numerics;

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

internal class And(params ObjectMatcher[] children) : ObjectMatcher
{
    public override bool Match(object? value)
    {
        foreach (var child in children)
            if (!child.Match(value)) return false;
        return true;
    }

    public static ObjectMatcher Create(IReadOnlyList<ObjectMatcher> children)
    {
        if (children is [var single]) return single;
        return new And([.. children]);
    }
}

internal class Or(params ObjectMatcher[] children) : ObjectMatcher
{
    public override bool Match(object? value)
    {
        foreach (var child in children)
            if (child.Match(value)) return true;
        return false;
    }
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

internal class Length(ObjectMatcher mather) : ObjectMatcher
{
    public override bool Match(object? value)
    {
        if (value is null) return false;

        if (value is string s) return mather.Match(s.Length);
        //todo: T[] 対応
        return false;
    }
}

internal static class Compare
{
    public static ObjectMatcher? Create(ComparisonType comparison, Type type, ReadOnlySpan<char> valueSpan)
    {
        if (type == typeof(int)) return CreateParsable<int>(comparison, valueSpan);
        if (type == typeof(uint)) return CreateParsable<uint>(comparison, valueSpan);
        if (type == typeof(long)) return CreateParsable<long>(comparison, valueSpan);
        if (type == typeof(ulong)) return CreateParsable<ulong>(comparison, valueSpan);
        if (type == typeof(short)) return CreateParsable<short>(comparison, valueSpan);
        if (type == typeof(ushort)) return CreateParsable<ushort>(comparison, valueSpan);
        if (type == typeof(byte)) return CreateParsable<byte>(comparison, valueSpan);
        if (type == typeof(sbyte)) return CreateParsable<sbyte>(comparison, valueSpan);
        if (type == typeof(float)) return CreateParsable<float>(comparison, valueSpan);
        if (type == typeof(double)) return CreateParsable<double>(comparison, valueSpan);
        if (type == typeof(decimal)) return CreateParsable<decimal>(comparison, valueSpan);
        if (type == typeof(TimeSpan)) return CreateParsable<TimeSpan>(comparison, valueSpan);
        if (type == typeof(DateTime)) return CreateParsable<DateTime>(comparison, valueSpan);
        if (type == typeof(DateTimeOffset)) return CreateParsable<DateTimeOffset>(comparison, valueSpan);
        if (type == typeof(bool)) return CreateBool(comparison, valueSpan);
        if (type == typeof(string)) return CreateString(comparison, valueSpan);

        // string, TimeSpan, DateTime(Offset)

        return null;
    }

    public static ObjectMatcher? CreateParsable<T>(ComparisonType comparison, ReadOnlySpan<char> valueSpan)
        where T : IComparable<T>, ISpanParsable<T>
    {
        if (!T.TryParse(valueSpan, null, out var value)) return null;
        return Compare<T>.Create(comparison, value);
    }

    public static ObjectMatcher? CreateBool(ComparisonType comparison, ReadOnlySpan<char> valueSpan)
    {
        if (valueSpan.Equals("true", StringComparison.OrdinalIgnoreCase)) return Compare<bool>.Create(comparison, true);
        if (valueSpan.Equals("false", StringComparison.OrdinalIgnoreCase)) return Compare<bool>.Create(comparison, false);
        return null;
    }

    public static ObjectMatcher? CreateString(ComparisonType comparison, ReadOnlySpan<char> valueSpan)
    {
        //todo: " とかから始まってたら \ unescape する

        valueSpan = valueSpan.Trim('"');
        valueSpan = valueSpan.Trim('\'');

        //todo: regex 認める？ それか正規表現マッチは文法自体変える？
        return Compare<string>.Create(comparison, valueSpan.ToString());
    }

    public static readonly ObjectMatcher IsNull = new IsNullMatcher();
    public static readonly ObjectMatcher IsNotNull = new IsNotNullMatcher();

    internal class IsNullMatcher : ObjectMatcher
    {
        public override bool Match(object? value) => value is null;
    }

    internal class IsNotNullMatcher : ObjectMatcher
    {
        public override bool Match(object? value) => value is { };
    }
}

internal class Compare<T>
    where T : IComparable<T>
{
    public static ObjectMatcher Create(ComparisonType comparison, T operand) => comparison switch
    {
        ComparisonType.Equal => new Equal(operand),
        ComparisonType.NotEqual => new NotEqual(operand),
        ComparisonType.GreaterThan => new GreaterThan(operand),
        ComparisonType.GreaterThanOrEqual => new GreaterThanOrEqual(operand),
        ComparisonType.LessThan => new LessThan(operand),
        ComparisonType.LessThanOrEqual => new LessThanOrEqual(operand),
        _ => throw new NotImplementedException()
    };

    private class Equal(T operand) : ObjectMatcher<T>
    {
        public override bool Match(T value) => value.CompareTo(operand) == 0;
    }

    private class NotEqual(T operand) : ObjectMatcher<T>
    {
        public override bool Match(T value) => value.CompareTo(operand) != 0;
    }

    private class GreaterThan(T operand) : ObjectMatcher<T>
    {
        public override bool Match(T value) => value.CompareTo(operand) > 0;
    }

    private class GreaterThanOrEqual(T operand) : ObjectMatcher<T>
    {
        public override bool Match(T value) => value.CompareTo(operand) >= 0;
    }

    private class LessThan(T operand) : ObjectMatcher<T>
    {
        public override bool Match(T value) => value.CompareTo(operand) < 0;
    }

    private class LessThanOrEqual(T operand) : ObjectMatcher<T>
    {
        public override bool Match(T value) => value.CompareTo(operand) <= 0;
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

internal static class FloatIntrinsic<T>
    where T : IFloatingPoint<T>
{
    public static ObjectMatcher? Create(string name, ObjectMatcher matcher) => name switch
    {
        IntrinsicNames.Ceiling => new Ceiling(matcher),
        IntrinsicNames.Floor => new Floor(matcher),
        IntrinsicNames.Round => new Round(matcher),
        _ => null
    };

    private class Ceiling(ObjectMatcher matcher) : ObjectMatcher<T>
    {
        public override bool Match(T value) => matcher.Match(T.ConvertToInteger<long>(T.Ceiling(value)));
    }

    private class Floor(ObjectMatcher matcher) : ObjectMatcher<T>
    {
        public override bool Match(T value) => matcher.Match(T.ConvertToInteger<long>(T.Floor(value)));
    }

    private class Round(ObjectMatcher matcher) : ObjectMatcher<T>
    {
        public override bool Match(T value) => matcher.Match(T.ConvertToInteger<long>(T.Round(value)));
    }
}
