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
        NodeType.Equal or NodeType.NotEqual
            or NodeType.LessThan or NodeType.LessThanOrEqual
            or NodeType.GreaterThan or NodeType.GreaterThanOrEqual
            or NodeType.Regex
            => Compare(node, root),
        _ => null, // 来ないはず
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

    private static Type? GetPropertyType(Node member, Type t)
    {
        if (member.IsNull) return t;

        var name = member.Span[0].Span.ToString();

        if (name == IntrinsicNames.Length) return typeof(int);

        if (name is IntrinsicNames.Ceiling
            or IntrinsicNames.Floor
            or IntrinsicNames.Round) return typeof(long);

        if (t.GetProperty(name) is not { } p) return null;

        return GetPropertyType(member.Left, p.PropertyType);
    }

    private static ObjectMatcher? Compare(Node node, Type root)
    {
        static ObjectMatcher? GetComparer(Node node, Type root)
        {
            var valueToken = node.Right.Span[0].Span;

            if (node.Type == NodeType.Regex) return RegexMatcher.Create(valueToken);

            var t = GetPropertyType(node.Left, root);
            if (t is null) return null;

            var compType = node.Type switch
            {
                NodeType.Equal => ComparisonType.Equal,
                NodeType.NotEqual => ComparisonType.NotEqual,
                NodeType.LessThan => ComparisonType.LessThan,
                NodeType.LessThanOrEqual => ComparisonType.LessThanOrEqual,
                NodeType.GreaterThan => ComparisonType.GreaterThan,
                NodeType.GreaterThanOrEqual => ComparisonType.GreaterThanOrEqual,
                _ => (ComparisonType)0,
            };

            return CodeCompletion.Emit.Compare.Create(compType, t, valueToken);
        }

        if (GetComparer(node, root) is not { } comp) return null;

        return MemberAccess(node.Left, root, comp);
    }

    private static ObjectMatcher? MemberAccess(Node node, Type root, ObjectMatcher comp)
    {
        if (node.IsNull) return comp;

        var name = node.Span[0].Span.ToString();

        if (name is ['.', ..]) return Intrinsic.Create(name, root, comp);

        if (root.GetProperty(name) is not { } p) return null;

        var child = MemberAccess(node.Left, p.PropertyType, comp);
        if (child is null) return null;

        return new Property(name, child);
    }
}

