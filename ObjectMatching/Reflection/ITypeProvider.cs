using System.Reflection;
using S = System.Reflection;

namespace ObjectMatching.Reflection;

/// <summary>
/// 型情報の取得方法をカスタマイズしたいことがあるので、
/// <see cref="Type.GetProperties"/> とかに1段インターフェイスを挟む。
/// </summary>
public interface ITypeProvider
{
    S.PropertyInfo[] GetProperties(Type type);
    S.PropertyInfo? GetProperty(Type type, string name);

    Type? GetElementType(Type t);
    bool IsNullable(S.PropertyInfo p);

    string? GetDerscription(Type t);
    string? GetDerscription(S.PropertyInfo p);
}

/// <summary>
/// デフォルトの <see cref="ITypeProvider"/> 実装。
/// ほぼ System.Reflection そのまま素通し。
/// </summary>
public class DefaultTypeProvider : ITypeProvider
{
    public S.PropertyInfo[] GetProperties(Type type) => type.GetProperties();

    public S.PropertyInfo? GetProperty(Type type, string name) => type.GetProperty(name);

    public Type? GetElementType(Type t)
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

    public bool IsNullable(S.PropertyInfo p)
    {
        var pt = p.PropertyType;

        // 値型
        if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Nullable<>)) return true;

        // T? じゃない値型
        if (pt.IsValueType) return false;

        // 参照型
        var c = new NullabilityInfoContext();
        var i = c.Create(p);
        return i.ReadState != S.NullabilityState.NotNull;
    }

    // Description 属性とかリフレクションで探す？
    public string? GetDerscription(Type t) => GetDescriptionFromAttribute(t) ?? t.Name;
    public string? GetDerscription(S.PropertyInfo p) => GetDescriptionFromAttribute(p);

    private static string? GetDescriptionFromAttribute(MemberInfo m)
    {
        var desc = m.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description;

        if (desc is null) return null;

        if (m is Type t && t.IsGenericType)
        {
            // 2引数以上のときもやる？
            foreach (var arg in t.GetGenericArguments())
            {
                var argDesc = GetDescriptionFromAttribute(arg) ?? arg.Name;
                if (argDesc is not null) desc += " - " + argDesc;
            }
        }

        return desc;
    }
}
