using CodeCompletion.Syntax;
using CodeCompletion.Text;
using CodeCompletion.Completion;
using CodeCompletion.Reflection;

namespace CodeCompletion.Emit;

internal class Emitter
{
    public static Func<object?, bool>? Emit(TextBuffer texts, TypeInfo type)
    {
        var node = Parser.Parse(texts);
        if (node.IsNull) return null;

        var m = Emit(node, type)!;
        if (m is null) return null;
        return m.Match;
    }

    private static ObjectMatcher? Emit(Node node, TypeInfo type) => node.Type switch
    {
        NodeType.Null => null,
        NodeType.Comma => new And(EmitChildren(node, type)),
        NodeType.Or => new Or(EmitChildren(node, type)),
        NodeType.And => new And(EmitChildren(node, type)),
        _ => Primary(node, type),
    };

    private static ObjectMatcher?[] EmitChildren(Node node, TypeInfo type)
    {
        var childNodes = node.GetChildren();
        var children = new ObjectMatcher?[childNodes.Length];
        for (var i = 0; i < children.Length; ++i)
        {
            children[i] = Emit(childNodes[i], type);
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

    private static ObjectMatcher? Primary(Node node, TypeInfo type)
    {
        if (node.IsNull) return null;

        // Array X = 1 とか書いて、C# でいう x.Array.Any(x => x.X == 1) 扱い。
        if (type.GetElementType() is { } et)
        {
            if (node.Span[0].Span is IntrinsicNames.Length)
            {
                var child = Emit(node.Left, new(typeof(int), type.TypeProvider));
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
            return Compare.Create(compType, type.Type, valueToken);
        }

        // member (A=1, B=2) みたいなのの、() の部分。
        // Comma, Or, And なので Emit 呼ぶ。
        if(node.Type != NodeType.Member) return Emit(node, type);

        var name = node.Span[0].Span.ToString();
        if (name is ['.', ..] && GetIntrinsicType(name) is { } it)
        {
            // .length とか。
            var child = Emit(node.Left, new(it, type.TypeProvider));
            if (child is null) return null;
            return Intrinsic.Create(name, type, child);
        }
        else
        {
            // プロパティアクセス。
            if (type.GetProperty(name) is not { } p) return null;

            var child = Emit(node.Left, p.PropertyType);
            if (child is null) return null;
            return new Property(name, child);
        }
    }
}

