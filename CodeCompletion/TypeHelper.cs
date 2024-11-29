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
}
