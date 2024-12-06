namespace ObjectMatching.Reflection;

/// <summary>
/// <see cref="Type"/> が必要なところには必ず <see cref="ITypeProvider"/> を伝搬させたいのでペアに。
/// </summary>
public readonly struct TypeInfoList(Type[] types, ITypeProvider typeProvider) : IReadOnlyList<TypeInfo>
{
    public TypeInfo this[int index] => new(types[index], typeProvider);

    public int Count => types.Length;

    public IEnumerator<TypeInfo> GetEnumerator()
    {
        foreach (var x in types) yield return new(x, typeProvider);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
