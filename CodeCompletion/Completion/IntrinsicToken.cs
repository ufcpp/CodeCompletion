
namespace CodeCompletion.Completion;

internal class IntrinsicNames
{
    public const string Length = ".length";
    public const string Ceiling = ".ceil";
    public const string Floor = ".floor";
    public const string Round = ".round";
    public const string Any = ".any";
    public const string All = ".all";

    // KeyValuePair は TValue 扱い(.Value を展開)にした上で、.Key の参照に .key intrinsic を用意するとかありかも。
    // その方が dictionary に対する条件書きやすく。
}
