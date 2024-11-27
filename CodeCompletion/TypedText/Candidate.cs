namespace CodeCompletion.TypedText;

public abstract class Candidate
{
    public abstract string? Text { get; }
    public abstract TypedToken GetToken();
}

/// <summary>
/// 常に固定の <see cref="Text"/>, <see cref="TypedToken"/> を返す <see cref="Candidate"/>。
/// </summary>
public class FixedCandidate(string? text, TypedToken token) : Candidate
{
    public override string? Text => text;
    public override TypedToken GetToken() => token;
}
