using CodeCompletion.Syntax;
using CodeCompletion.Text;
using CodeCompletion.Completion;

namespace CodeCompletion.Emit;

internal class Emitter
{
    public static Func<object?, bool>? Emit(TextBuffer texts, Type root)
    {
        var node = Parser.Parse(texts);
        if (node.IsNull) return null;

        var m = Emit(node, root)!;
        if (m is null) return null;
        return m.Match;
    }

    private static ObjectMatcher? Emit(Node node, Type root) => node.Type switch
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
        if (TypeHelper.GetElementType(t) is { } et)
        {
            if (node.Span[0].Span is IntrinsicNames.Length)
            {
                var child = Emit(node.Left, typeof(int));
                if (child is null) return null;
                return new ArrayLength(child);
            }

            bool all = false;
            if (node.Span[0].Span is IntrinsicNames.Any)
            {
                node = node.Left;
                // 今、 Array X と Array .any X は全く同じ扱いになってるけど、
                // 下記 todo の通り、 = null, != null の挙動は変えた方がいいかも。
            }
            else if (node.Span[0].Span is IntrinsicNames.All)
            {
                node = node.Left;
                all = true;
            }

            //todo: = null, != null だけは配列インスタンス自体の null 判定にした方がいいかも。
            // (Any(x => x != null) の意味にしたければ Array .any != null とか書く。)

            var elem = Emit(node, et);
            if (elem is null) return null;
            return all ? new ArrayAll(elem) : new ArrayAny(elem);
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
            var child = Emit(node.Left, it);
            if (child is null) return null;
            return Intrinsic.Create(name, t, child);
        }
        else
        {
            // プロパティアクセス。
            if (t.GetProperty(name) is not { } p) return null;

            var child = Emit(node.Left, p.PropertyType);
            if (child is null) return null;
            return new Property(name, child);
        }
    }
}

