namespace CodeCompletion.TypedText;

public class LiteralToken(Type type) : ValueToken
{
    public override string ToString() => $"Literal {type.Name}";
}
