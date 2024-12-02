namespace CodeCompletion.Reflection;

/// <summary>
/// <see cref="ITypeProvider"/> と違って virtual にする必要なさそうなものは拡張メソッド提供。
/// (型がらみの判定だけ virtual にしても、その後の emit とかで困る。)
/// </summary>
internal static class TypeHelper
{
    public static ComparableTypeCategory GetComparableType(this Type type)
    {
        if (type == typeof(string)) return ComparableTypeCategory.String;
        if (type == typeof(bool)) return ComparableTypeCategory.Bool;
        if (type == typeof(float)
            || type == typeof(double)
            || type == typeof(decimal)
            ) return ComparableTypeCategory.Float;
        if (type == typeof(int)
            || type == typeof(long)
            || type == typeof(short)
            || type == typeof(byte)
            || type == typeof(uint)
            || type == typeof(ulong)
            || type == typeof(ushort)
            || type == typeof(sbyte)
            ) return ComparableTypeCategory.Integer;
        if (type == typeof(TimeSpan)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(DateOnly)
            || type == typeof(TimeOnly)
            ) return ComparableTypeCategory.Comparable;

        var i = HasInterface(type);

        // 時刻系とかもこの条件で拾えるけど、判定が重たいので分けてる。
        if (i.HasFlag(InterfaceType.ISpanParsable | InterfaceType.IComparable))
            return ComparableTypeCategory.Comparable;

        if (i.HasFlag(InterfaceType.ISpanParsable | InterfaceType.IEquatable))
            return ComparableTypeCategory.Equatable;

        return ComparableTypeCategory.None;
    }

    public static InterfaceType HasInterface(Type type)
    {
        InterfaceType x = default;
        foreach (var i in type.GetInterfaces())
        {
            if (!i.IsGenericType) continue;
            var args = i.GenericTypeArguments;
            if (args.Length != 1 || args[0] != type) continue;
            if (i.GetGenericTypeDefinition() == typeof(IComparable<>)) x |= InterfaceType.IComparable;
            if (i.GetGenericTypeDefinition() == typeof(IEquatable<>)) x |= InterfaceType.IEquatable;
            if (i.GetGenericTypeDefinition() == typeof(ISpanParsable<>)) x |= InterfaceType.ISpanParsable;
        }
        return x;
    }
}

/// <summary>
/// メンバー参照じゃなくて、 = value とかを出力したい型かどうかの判定。
/// 候補にどの演算子を出すかも型によって違うので、enum で判定結果を返す。
/// </summary>
internal enum ComparableTypeCategory
{
    None,
    String,
    Bool,
    Float,
    Integer,
    Comparable,
    Equatable,
}


[Flags]
internal enum InterfaceType
{
    ISpanParsable = 1,
    IComparable = 2,
    IEquatable = 4,
}
