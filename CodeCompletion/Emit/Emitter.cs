using CodeCompletion.Syntax;
using CodeCompletion.TypedText;

namespace CodeCompletion.Emit;

internal class Emitter
{
    public static ObjectMatcher? Emit(Node node, Type root) => node.Type switch
    {
        NodeType.Comma => new And(EmitChildren(node, root)),
        NodeType.Or => new Or(EmitChildren(node, root)),
        NodeType.And => new And(EmitChildren(node, root)),
        _ => Primary(node, root),
    };

    private static ObjectMatcher?[] EmitChildren(Node node, Type root)
    {
        var childNodes = node.GetChildren();
        var children = new ObjectMatcher?[childNodes.Length];
        for (var i = 0; i < children.Length; ++i)
        {
            children[i] = Emit(childNodes[i], root);
        }
        return children;
    }

    private static Type? GetIntrinsicType(ReadOnlySpan<char> name) => name switch
    {
        IntrinsicNames.Length => typeof(int),
        IntrinsicNames.Ceiling
        or IntrinsicNames.Floor
        or IntrinsicNames.Round => typeof(long),
        _ => null,
    };

    private static ObjectMatcher? Primary(Node node, Type t)
    {
        if (node.IsNull) return null;

        // Array X = 1 とか書いて、C# でいう x.Array.Any(x => x.X == 1) 扱い。

        //todo: = null, != null だけは配列インスタンス自体の null 判定にした方がいいかも。
        // (Any(x => x != null) の意味にしたければ Array .any != null とか書けるようにする。)
        if (t.IsArray)
        {
            var et = t.GetElementType()!;
            var elem = Primary(node, et);
            if (elem is null) return null;
            return new ArrayAny(elem);
        }

        // = とか > とか。
        if (node.Type.IsComparison())
        {
            var valueToken = node.Span[1].Span;

            if (node.Type == NodeType.Regex) return RegexMatcher.Create(valueToken);

            var compType = node.Type.ToComparisonType();
            return Compare.Create(compType, t, valueToken);
        }

        // member (A=1, B=2) みたいなのの、() の部分。
        // Comma, Or, And なので Emit 呼ぶ。
        if(node.Type != NodeType.Member) return Emit(node, t);

        var name = node.Span[0].Span.ToString();
        if (name is ['.', ..] && GetIntrinsicType(name) is { } it)
        {
            // .length とか。
            var child = Primary(node.Left, it);
            if (child is null) return null;
            return Intrinsic.Create(name, t, child);
        }
        else
        {
            // プロパティアクセス。
            if (t.GetProperty(name) is not { } p) return null;

            var child = Primary(node.Left, p.PropertyType);
            if (child is null) return null;
            return new Property(name, child);
        }
    }
}

