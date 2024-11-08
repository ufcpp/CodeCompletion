using CodeCompletion.Semantics;

namespace CodeCompletion.Emit;

internal abstract class ObjectMatcher
{
    public abstract bool Match(object? value);
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

internal class IsNull : ObjectMatcher
{
    public override bool Match(object? value) => value is null;
}

internal class IsNotNull : ObjectMatcher
{
    public override bool Match(object? value) => value is { };
}

internal static class Compare
{
    public static ObjectMatcher? Create(ComparisonType comparison, Type type, ReadOnlySpan<char> valueSpan)
    {
        if (type == typeof(int)) return CreateParsable<int>(comparison, valueSpan);
        if (type == typeof(uint)) return CreateParsable<int>(comparison, valueSpan);
        if (type == typeof(long)) return CreateParsable<int>(comparison, valueSpan);
        if (type == typeof(ulong)) return CreateParsable<int>(comparison, valueSpan);
        if (type == typeof(short)) return CreateParsable<int>(comparison, valueSpan);
        if (type == typeof(ushort)) return CreateParsable<int>(comparison, valueSpan);
        if (type == typeof(byte)) return CreateParsable<int>(comparison, valueSpan);
        if (type == typeof(sbyte)) return CreateParsable<int>(comparison, valueSpan);
        if (type == typeof(float)) return CreateParsable<int>(comparison, valueSpan);
        if (type == typeof(double)) return CreateParsable<int>(comparison, valueSpan);
        if (type == typeof(decimal)) return CreateParsable<int>(comparison, valueSpan);
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
        //todo: " とかから始まってたら unescape する
        //todo: regex 認める？ それか正規表現マッチは文法自体変える？
        return Compare<string>.Create(comparison, valueSpan.ToString());
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

    private class Equal(T operand) : ObjectMatcher
    {
        public override bool Match(object? value) => value is T t && t.CompareTo(operand) == 0;
    }

    private class NotEqual(T operand) : ObjectMatcher
    {
        public override bool Match(object? value) => value is T t && t.CompareTo(operand) != 0;
    }

    private class GreaterThan(T operand) : ObjectMatcher
    {
        public override bool Match(object? value) => value is T t && t.CompareTo(operand) > 0;
    }

    private class GreaterThanOrEqual(T operand) : ObjectMatcher
    {
        public override bool Match(object? value) => value is T t && t.CompareTo(operand) >= 0;
    }

    private class LessThan(T operand) : ObjectMatcher
    {
        public override bool Match(object? value) => value is T t && t.CompareTo(operand) < 0;
    }

    private class LessThanOrEqual(T operand) : ObjectMatcher
    {
        public override bool Match(object? value) => value is T t && t.CompareTo(operand) <= 0;
    }
}

