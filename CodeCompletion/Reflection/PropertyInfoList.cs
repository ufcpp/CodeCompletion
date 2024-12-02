using S = System.Reflection;

namespace CodeCompletion.Reflection;

/// <summary>
/// <see cref="S.PropertyInfo"/> が必要なところには必ず <see cref="ITypeProvider"/> を伝搬させたいのでペアに。
/// </summary>
public readonly struct PropertyInfoList(S.PropertyInfo[] properties, ITypeProvider typeProvider) : IReadOnlyList<PropertyInfo>
{
    public PropertyInfo this[int index] => new(properties[index], typeProvider);

    public int Count => properties.Length;

    public IEnumerator<PropertyInfo> GetEnumerator()
    {
        foreach (var x in properties) yield return new(x, typeProvider);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
