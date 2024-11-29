using System.Reflection;

namespace CodeCompletion;

internal class TypeHelper
{
    public static Type? GetElementType(Type t)
    {
        if (t == typeof(string)) return null;

        if (t.IsArray) return t.GetElementType();

        // 無条件 IEnumerable でいい？
        // もうちょっと絞る？
        // 実際、 string を誤検知する(のでメソッド先頭に特殊分岐がある)。
        foreach (var i in t.GetInterfaces())
        {
            if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)) return i.GetGenericArguments()[0];
        }

        return null;
    }

    public static bool IsNullable(PropertyInfo p)
    {
        var pt = p.PropertyType;

        // 値型
        if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Nullable<>)) return true;

        // T? じゃない値型
        if (pt.IsValueType) return false;

        // 参照型
        var c = new NullabilityInfoContext();
        var i = c.Create(p);
        return i.ReadState != NullabilityState.NotNull;
    }

    public static HasInterface HasInterface(Type type)
    {
        HasInterface x = default;
        foreach (var i in type.GetInterfaces())
        {
            if (!i.IsGenericType) continue;
            var args = i.GenericTypeArguments;
            if (args.Length != 1 || args[0] != type) continue;
            if (i.GetGenericTypeDefinition() == typeof(IComparable<>)) x |= CodeCompletion.HasInterface.IComparable;
            if (i.GetGenericTypeDefinition() == typeof(IEquatable<>)) x |= CodeCompletion.HasInterface.IEquatable;
            if (i.GetGenericTypeDefinition() == typeof(ISpanParsable<>)) x |= CodeCompletion.HasInterface.ISpanParsable;
        }
        return x;
    }
}

[Flags]
internal enum HasInterface
{
    ISpanParsable = 1,
    IComparable = 2,
    IEquatable = 4,
}
