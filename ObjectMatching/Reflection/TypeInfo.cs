namespace ObjectMatching.Reflection;

/// <summary>
/// <see cref="System.Type"/> が必要なところには必ず <see cref="ITypeProvider"/> を伝搬させたいのでペアに。
/// </summary>
public readonly struct TypeInfo(Type type, ITypeProvider typeProvider)
{
    public Type Type => type;
    public ITypeProvider TypeProvider => typeProvider;

    public PropertyInfoList GetProperties() => new(typeProvider.GetProperties(type), typeProvider);
    public PropertyInfo? GetProperty(string name) => typeProvider.GetProperty(type, name) is { } p ? new (p, typeProvider) : null;
    public TypeInfo? GetElementType() => typeProvider.GetElementType(type) is { } et ? new (et, typeProvider) : null;
    public string Name => type!.Name;
    public string? Description => typeProvider.GetDerscription(type);
    public TypeInfoList GenericArguments => new(type.GetGenericArguments(), typeProvider);

    /// <summary>
    /// <see cref="KeyValuePair{TKey, TValue}"/> のとき、TValue の型を返す。
    /// そうでないとき null。
    /// </summary>
    public TypeInfoList? GetKeyValuePairType()
    {
        var t = type.GetKeyValuePairType();
        return t is null ? null : new(t, typeProvider);
    }
}
