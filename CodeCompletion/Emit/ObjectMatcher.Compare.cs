using CodeCompletion.Completion;
using CodeCompletion.Reflection;
using System.Runtime.CompilerServices;

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
            if (Nullable.GetUnderlyingType(type) is { } ut)
            {
                var inner = Compare.Create(comparison, ut, valueSpan);
                var t = typeof(NullableMatcher<>).MakeGenericType(ut);
                return (ObjectMatcher)Activator.CreateInstance(t, inner)!;
            }

            Type? genericType = null;

            if (type.IsEnum)
            {
                genericType = typeof(CompreEnum<,>).MakeGenericType(type, type.GetEnumUnderlyingType());
            }

            var i = TypeHelper.HasInterface(type);
            if (i.HasFlag(InterfaceType.ISpanParsable | InterfaceType.IComparable))
            {
                genericType = typeof(ComparableParseable<>).MakeGenericType(type);
            }
            else if (i.HasFlag(InterfaceType.ISpanParsable | InterfaceType.IEquatable))
            {
                genericType = typeof(EquatableParseable<>).MakeGenericType(type);
            }

            return InvokeCreate(comparison, valueSpan, genericType);
        }

        private static ObjectMatcher? InvokeCreate(ComparisonType comparison, ReadOnlySpan<char> valueSpan, Type? t)
        {
            if (t is null) return null;
            var m = t.GetMethod("Create", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)!;
            var f = m.CreateDelegate<Func<ComparisonType, ReadOnlySpan<char>, ObjectMatcher?>>();
            return f(comparison, valueSpan);
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

        private class Equal(T operand) : ObjectMatcher<T> { public override bool Match(T value) => value.CompareTo(operand) == 0; }
        private class NotEqual(T operand) : ObjectMatcher<T> { public override bool Match(T value) => value.CompareTo(operand) != 0; }
        private class GreaterThan(T operand) : ObjectMatcher<T> { public override bool Match(T value) => value.CompareTo(operand) > 0; }
        private class GreaterThanOrEqual(T operand) : ObjectMatcher<T> { public override bool Match(T value) => value.CompareTo(operand) >= 0; }
        private class LessThan(T operand) : ObjectMatcher<T> { public override bool Match(T value) => value.CompareTo(operand) < 0; }
        private class LessThanOrEqual(T operand) : ObjectMatcher<T> { public override bool Match(T value) => value.CompareTo(operand) <= 0; }
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

        private class Equal(T operand) : ObjectMatcher<T> { public override bool Match(T value) => value.Equals(operand); }
        private class NotEqual(T operand) : ObjectMatcher<T> { public override bool Match(T value) => !value.Equals(operand); }
    }

    private class CompreEnum<TEnum, TUnderlying>
        where TEnum : struct, Enum
        where TUnderlying : IComparable<TUnderlying>, ISpanParsable<TUnderlying>
    {
        public static ObjectMatcher? Create(ComparisonType comparison, ReadOnlySpan<char> valueSpan)
        {
            valueSpan = StringHelper.Unescape(valueSpan); // Date 系、"" で囲まないと入力できない。

            // 名前で parse
            if (!Enum.TryParse<TEnum>(valueSpan, out var value))
            {
                // 数値で parse
                if (TUnderlying.TryParse(valueSpan, null, out var x)) value = Unsafe.BitCast<TUnderlying, TEnum>(x);
                else return null;
            }
            return Comparable.Create(comparison, value);
        }
    }

    private class Comparable
    {
        public static ObjectMatcher Create(ComparisonType comparison, IComparable operand) => comparison switch
        {
            ComparisonType.Equal => new Equal(operand),
            ComparisonType.NotEqual => new NotEqual(operand),
            ComparisonType.GreaterThan => new GreaterThan(operand),
            ComparisonType.GreaterThanOrEqual => new GreaterThanOrEqual(operand),
            ComparisonType.LessThan => new LessThan(operand),
            ComparisonType.LessThanOrEqual => new LessThanOrEqual(operand),
            _ => throw new NotImplementedException() // 他は null 返して単に無視してるのが多い
        };

        private class Equal(IComparable operand) : ObjectMatcher<IComparable> { public override bool Match(IComparable value) => value.CompareTo(operand) == 0; }
        private class NotEqual(IComparable operand) : ObjectMatcher<IComparable> { public override bool Match(IComparable value) => value.CompareTo(operand) != 0; }
        private class GreaterThan(IComparable operand) : ObjectMatcher<IComparable> { public override bool Match(IComparable value) => value.CompareTo(operand) > 0; }
        private class GreaterThanOrEqual(IComparable operand) : ObjectMatcher<IComparable> { public override bool Match(IComparable value) => value.CompareTo(operand) >= 0; }
        private class LessThan(IComparable operand) : ObjectMatcher<IComparable> { public override bool Match(IComparable value) => value.CompareTo(operand) < 0; }
        private class LessThanOrEqual(IComparable operand) : ObjectMatcher<IComparable> { public override bool Match(IComparable value) => value.CompareTo(operand) <= 0; }
    }

    private class NullableMatcher<T>(ObjectMatcher matcher) : ObjectMatcher<T?>
        where T : struct
    {
        // null との比較は IsNull, IsNotNull に行くはず。
        public override bool Match(T? value) => value is { } x && matcher.Match(x);
    }
}
