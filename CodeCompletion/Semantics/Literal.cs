namespace CodeCompletion.Semantics;

public class LiteralNode(Type type) : ValueNode
{
    public override string ToString() => $"Literal {type.Name}";
}
