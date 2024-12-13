using ObjectMatching.Reflection;
using System.Runtime.CompilerServices;

namespace ObjectMatching.Emit;

using Res = Result<ObjectMatcher, BoxedErrorCode>;

internal static class Compare
{
    public static Res Create(ComparisonType comparison, Type type, ReadOnlySpan<char> valueSpan)
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

        return BoxedErrorCode.UnsupportedType;
    }

    public static Res CreateParsable<T>(ComparisonType comparison, ReadOnlySpan<char> valueSpan)
        where T : IComparable<T>, ISpanParsable<T>
        => ComparableParseable<T>.Create(comparison, valueSpan);

    public static Res CreateBool(ComparisonType comparison, ReadOnlySpan<char> valueSpan)
    {
        if (valueSpan.Equals("true", StringComparison.OrdinalIgnoreCase)) return Comparable<bool>.Create(comparison, true);
        if (valueSpan.Equals("false", StringComparison.OrdinalIgnoreCase)) return Comparable<bool>.Create(comparison, false);
        return BoxedErrorCode.InvalidOperand;
    }

    public static Res CreateString(ComparisonType comparison, ReadOnlySpan<char> valueSpan)
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
        public static Res Create(ComparisonType comparison, Type type, ReadOnlySpan<char> valueSpan)
        {
            if (Nullable.GetUnderlyingType(type) is { } ut)
            {
                return Compare.Create(comparison, ut, valueSpan);
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

            if (genericType is null) return BoxedErrorCode.UnsupportedType;

            return InvokeCreate(comparison, valueSpan, genericType);
        }

        private static Res InvokeCreate(ComparisonType comparison, ReadOnlySpan<char> valueSpan, Type t)
        {
            var m = t.GetMethod("Create", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)!;
            var f = m.CreateDelegate<Func<ComparisonType, ReadOnlySpan<char>, Res>>();
            return f(comparison, valueSpan);
        }
    }

    private class ComparableParseable<T>
        where T : IComparable<T>, ISpanParsable<T>
    {
        public static Res Create(ComparisonType comparison, ReadOnlySpan<char> valueSpan)
        {
            valueSpan = StringHelper.Unescape(valueSpan); // Date 系、"" で囲まないと入力できない。

            if (!T.TryParse(valueSpan, null, out var value)) return BoxedErrorCode.InvalidOperand;
            return Comparable<T>.Create(comparison, value);
        }
    }

    private class Comparable<T>
        where T : IComparable<T>
    {
        public static Res Create(ComparisonType comparison, T operand) => comparison switch
        {
            ComparisonType.Equal => new Equal(operand),
            ComparisonType.NotEqual => new NotEqual(operand),
            ComparisonType.GreaterThan => new GreaterThan(operand),
            ComparisonType.GreaterThanOrEqual => new GreaterThanOrEqual(operand),
            ComparisonType.LessThan => new LessThan(operand),
            ComparisonType.LessThanOrEqual => new LessThanOrEqual(operand),
            _ => BoxedErrorCode.InvalidOperator,
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
        public static Res Create(ComparisonType comparison, ReadOnlySpan<char> valueSpan)
        {
            valueSpan = StringHelper.Unescape(valueSpan); // Date 系、"" で囲まないと入力できない。

            if (!T.TryParse(valueSpan, null, out var value)) return BoxedErrorCode.InvalidOperand;
            return Equatable<T>.Create(comparison, value);
        }
    }

    private class Equatable<T>
        where T : IEquatable<T>
    {
        public static Res Create(ComparisonType comparison, T operand) => comparison switch
        {
            ComparisonType.Equal => new Equal(operand),
            ComparisonType.NotEqual => new NotEqual(operand),
            _ => BoxedErrorCode.InvalidOperator,
        };

        private class Equal(T operand) : ObjectMatcher<T> { public override bool Match(T value) => value.Equals(operand); }
        private class NotEqual(T operand) : ObjectMatcher<T> { public override bool Match(T value) => !value.Equals(operand); }
    }

    private class CompreEnum<TEnum, TUnderlying>
        where TEnum : struct, Enum
        where TUnderlying : struct, IComparable<TUnderlying>, ISpanParsable<TUnderlying>
    {
        public static Res Create(ComparisonType comparison, ReadOnlySpan<char> valueSpan)
        {
            valueSpan = StringHelper.Unescape(valueSpan); // Date 系、"" で囲まないと入力できない。

            // 名前で parse
            if (!Enum.TryParse<TEnum>(valueSpan, out var value))
            {
                // 数値で parse
                if (TUnderlying.TryParse(valueSpan, null, out var x)) value = Unsafe.BitCast<TUnderlying, TEnum>(x);
                else return BoxedErrorCode.InvalidOperand;
            }
            return Comparable.Create(comparison, value);
        }
    }

    private class Comparable
    {
        public static Res Create(ComparisonType comparison, Enum operand) => comparison switch
        {
            ComparisonType.Equal => new Equal(operand),
            ComparisonType.NotEqual => new NotEqual(operand),
            ComparisonType.GreaterThan => new GreaterThan(operand),
            ComparisonType.GreaterThanOrEqual => new GreaterThanOrEqual(operand),
            ComparisonType.LessThan => new LessThan(operand),
            ComparisonType.LessThanOrEqual => new LessThanOrEqual(operand),
            ComparisonType.Tilde => new HasFlag(operand),
            _ => BoxedErrorCode.InvalidOperator,
        };

        private class Equal(Enum operand) : ObjectMatcher<Enum> { public override bool Match(Enum value) => value.CompareTo(operand) == 0; }
        private class NotEqual(Enum operand) : ObjectMatcher<Enum> { public override bool Match(Enum value) => value.CompareTo(operand) != 0; }
        private class GreaterThan(Enum operand) : ObjectMatcher<Enum> { public override bool Match(Enum value) => value.CompareTo(operand) > 0; }
        private class GreaterThanOrEqual(Enum operand) : ObjectMatcher<Enum> { public override bool Match(Enum value) => value.CompareTo(operand) >= 0; }
        private class LessThan(Enum operand) : ObjectMatcher<Enum> { public override bool Match(Enum value) => value.CompareTo(operand) < 0; }
        private class LessThanOrEqual(Enum operand) : ObjectMatcher<Enum> { public override bool Match(Enum value) => value.CompareTo(operand) <= 0; }
        private class HasFlag(Enum operand) : ObjectMatcher<Enum> { public override bool Match(Enum value) => value.HasFlag(operand); }
    }
}
