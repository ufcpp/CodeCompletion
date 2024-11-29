using System.Diagnostics;
using System.Reflection;

namespace CodeCompletion.Completion;

/// <summary>
/// プロパティ情報。
/// </summary>
/// <remarks>
/// <see cref="PropertyInfo"/> そのまま使う案もあったけど、
/// intrinsics の時に似せ PropertyInfo 作るのが面倒だし、使う情報は型、名前、nullability だけなので。
/// </remarks>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal readonly record struct Property(Type PropertyType, string Name, bool IsNullable) //todo: 配列の要素、型引数等
{
    public Property(PropertyInfo p) : this(p.PropertyType, p.Name, TypeHelper.IsNullable(p)) { }

    internal string GetDebuggerDisplay() => $"{Name}:{PropertyType.Name}{(IsNullable ? "?" : "")}";
}

/// <summary>
/// 補完候補を出すのに必要なプロパティ情報。
/// </summary>
/// <param name="Direct">直近のプロパティ。</param>
/// <param name="Parenthesis">一番近い ( 直前のプロパティ。</param>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal readonly record struct PropertyHierarchy(
    Property Direct,
    Property Parenthesis
    )
{
    private string GetDebuggerDisplay() => $"{Direct.GetDebuggerDisplay()} / {Parenthesis.GetDebuggerDisplay()}";
}
