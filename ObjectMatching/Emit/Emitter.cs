using CodeCompletion.Text;
using ObjectMatching.Syntax;
using ObjectMatching.Reflection;

namespace ObjectMatching.Emit;

using Res = Result<ObjectMatcher, Error>;
using Func = Result<Func<object?, bool>, Error>;

internal class Emitter
{
    public static Func Emit(TextBuffer texts, TypeInfo type)
    {
        var node = Parser.Parse(texts);
        if (node.IsNull) return Error.UnknownSyntaxError;

        var m = Emit(node, type)!;
        if (m.Value is not { } matcher) return m.Error!;
        return new(matcher.Match);
    }

    private static Res Emit(Node node, TypeInfo type) => node.Type switch
    {
        NodeType.Null => Error.UnknownSyntaxError,
        NodeType.Comma or NodeType.Or or NodeType.And => EmitChildren(node, type),
        _ => Primary(node, type),
    };

    private static Res EmitChildren(Node node, TypeInfo type)
    {
        var childNodes = node.GetChildren();
        var children = new ObjectMatcher?[childNodes.Length];
        for (var i = 0; i < children.Length; ++i)
        {
            var child = Emit(childNodes[i], type);
            if (child.Value is not { } c) return child;
            children[i] = c;
        }

        return node.Type switch
        {
            NodeType.Or => new Or(children),
            NodeType.And or NodeType.Comma => new And(children),
            _ => BoxedErrorCode.InvalidOperator.With(node),
        };
    }

    private static Type? GetIntrinsicType(ReadOnlySpan<char> name) => name switch
    {
        IntrinsicNames.Length => typeof(int),
        IntrinsicNames.Ceiling
        or IntrinsicNames.Floor
        or IntrinsicNames.Round => typeof(long),
        _ => null,
    };

    private static Res Primary(Node node, TypeInfo type)
    {
        if (node.IsNull) return Error.UnknownSyntaxError;

        // Array X = 1 とか書いて、C# でいう x.Array.Any(x => x.X == 1) 扱い。
        if (type.GetElementType() is { } et)
        {
            if (node.Span[0].Span is IntrinsicNames.Length)
            {
                var child = Emit(node.Left, new(typeof(int), type.TypeProvider));
                if (child.Value is not { } c) return child;
                return new ArrayLength(c);
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
            if (elem.Value is not { } e) return elem;
            return all ? new ArrayAll(e) : new ArrayAny(e);
        }

        // = とか > とか。
        if (node.Type.IsComparison())
        {
            var valueToken = node.Span[1].Span;

            if (node.Type == NodeType.Regex
                && type.Type == typeof(string))
                return RegexMatcher.Create(valueToken).With(node);

            var compType = node.Type.ToComparisonType();
            return Compare.Create(compType, type.Type, valueToken).With(node);
        }

        // member (A=1, B=2) みたいなのの、() の部分。
        // Comma, Or, And なので Emit 呼ぶ。
        if(node.Type != NodeType.Member) return Emit(node, type);

        var name = node.Span[0].Span.ToString();
        if (name is ['.', ..] && GetIntrinsicType(name) is { } it)
        {
            // .length とか。
            var child = Emit(node.Left, new(it, type.TypeProvider));
            if (child.Value is not { } c) return child;
            return Intrinsic.Create(name, type, c).With(node);
        }
        else
        {
            // プロパティアクセス。
            if (type.GetProperty(name) is not { } p) return BoxedErrorCode.PropertyNotFound.With(node);

            var child = Emit(node.Left, p.PropertyType);
            if (child.Value is not { } c) return child;
            return new Property(name, c);
        }
    }
}

