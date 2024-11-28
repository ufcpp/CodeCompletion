namespace CodeCompletion;

internal class TypeHelper
{
    public static Type? GetElementType(Type t)
    {
        if (t.IsArray) return t.GetElementType();

        // 無条件 IEnumerable でいい？
        // もうちょっと絞る？
        foreach (var i in t.GetInterfaces())
        {
            if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)) return i.GetGenericArguments()[0];
        }

        return null;
    }
}
