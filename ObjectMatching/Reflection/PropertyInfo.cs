using S = System.Reflection;

namespace ObjectMatching.Reflection;

/// <summary>
/// <see cref="S.PropertyInfo"/> が必要なところには必ず <see cref="ITypeProvider"/> を伝搬させたいのでペアに。
/// </summary>
public readonly struct PropertyInfo(S.PropertyInfo property, ITypeProvider typeProvider)
{
    public bool IsNullable() => typeProvider.IsNullable(property);

    public TypeInfo PropertyType => new(property.PropertyType, typeProvider);
    public string Name => property.Name;
}
