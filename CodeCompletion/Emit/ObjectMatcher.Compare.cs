using CodeCompletion.Completion;
using CodeCompletion.Reflection;

namespace CodeCompletion.Emit;

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
        if (type == typeof(bool)) return CreateBool(comparison, valueSpan);
        if (type == typeof(string)) return CreateString(comparison, valueSpan);

        if (comparison == ComparisonType.Equal && valueSpan is "null") return IsNull;
        if (comparison == ComparisonType.NotEqual && valueSpan is "null") return IsNotNull;

        if (Dynamic.Create(comparison, type, valueSpan) is { } c) return c;

        return null;
    }

    public static ObjectMatcher? CreateParsable<T>(ComparisonType comparison, ReadOnlySpan<char> valueSpan)
        where T : IComparable<T>, ISpanParsable<T>
        => ComparableParseable<T>.Create(comparison, valueSpan);

    public static ObjectMatcher? CreateBool(ComparisonType comparison, ReadOnlySpan<char> valueSpan)
    {
        if (valueSpan.Equals("true", StringComparison.OrdinalIgnoreCase)) return Comparable<bool>.Create(comparison, true);
        if (valueSpan.Equals("false", StringComparison.OrdinalIgnoreCase)) return Comparable<bool>.Create(comparison, false);
        return null;
    }

    public static ObjectMatcher? CreateString(ComparisonType comparison, ReadOnlySpan<char> valueSpan)
    {
        valueSpan = StringHelper.Unescape(valueSpan);
        return Comparable<string>.Create(comparison, valueSpan.ToString());
    }

    public static readonly ObjectMatcher IsNull = new IsNullMatcher();
    public static readonly ObjectMatcher IsNotNull = new IsNotNullMatcher();

    private class IsNullMatcher : ObjectMatcher
    {
        public override bool Match(object? value) => value is null;
    }

    private class IsNotNullMatcher : ObjectMatcher
    {
        public override bool Match(object? value) => value is { };
    }

    private class Dynamic
    {
        public static ObjectMatcher? Create(ComparisonType comparison, Type type, ReadOnlySpan<char> valueSpan)
        {
            var i = TypeHelper.HasInterface(type);
            if (i.HasFlag(InterfaceType.ISpanParsable | InterfaceType.IComparable))
            {
                var t = typeof(ComparableParseable<>).MakeGenericType(type);
                var m = t.GetMethod(nameof(ComparableParseable<int>.Create), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)!;
                var f = m.CreateDelegate<Func<ComparisonType, ReadOnlySpan<char>, ObjectMatcher?>>();
                return f(comparison, valueSpan);
            }
            else if (i.HasFlag(InterfaceType.ISpanParsable | InterfaceType.IEquatable))
            {
                var t = typeof(EquatableParseable<>).MakeGenericType(type);
                var m = t.GetMethod(nameof(ComparableParseable<int>.Create), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)!;
                var f = m.CreateDelegate<Func<ComparisonType, ReadOnlySpan<char>, ObjectMatcher?>>();
                return f(comparison, valueSpan);
            }

            return null;
        }
    }

    private class ComparableParseable<T>
        where T : IComparable<T>, ISpanParsable<T>
    {
        public static ObjectMatcher? Create(ComparisonType comparison, ReadOnlySpan<char> valueSpan)
        {
            valueSpan = StringHelper.Unescape(valueSpan); // Date 系、"" で囲まないと入力できない。

            if (!T.TryParse(valueSpan, null, out var value)) return null;
            return Comparable<T>.Create(comparison, value);
        }
    }

    private class Comparable<T>
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
            _ => throw new NotImplementedException() // 他は null 返して単に無視してるのが多い
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

    private class EquatableParseable<T>
        where T : IEquatable<T>, ISpanParsable<T>
    {
        public static ObjectMatcher? Create(ComparisonType comparison, ReadOnlySpan<char> valueSpan)
        {
            valueSpan = StringHelper.Unescape(valueSpan); // Date 系、"" で囲まないと入力できない。

            if (!T.TryParse(valueSpan, null, out var value)) return null;
            return Equatable<T>.Create(comparison, value);
        }
    }

    private class Equatable<T>
        where T : IEquatable<T>
    {
        public static ObjectMatcher Create(ComparisonType comparison, T operand) => comparison switch
        {
            ComparisonType.Equal => new Equal(operand),
            ComparisonType.NotEqual => new NotEqual(operand),
            _ => throw new NotImplementedException() // 他は null 返して単に無視してるのが多い
        };

        private class Equal(T operand) : ObjectMatcher<T>
        {
            public override bool Match(T value) => value.Equals(operand);
        }

        private class NotEqual(T operand) : ObjectMatcher<T>
        {
            public override bool Match(T value) => !value.Equals(operand);
        }
    }
}
