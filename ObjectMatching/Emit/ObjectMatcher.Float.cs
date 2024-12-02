using System.Numerics;

namespace ObjectMatching.Emit;

internal static class FloatIntrinsic<T>
    where T : IFloatingPoint<T>
{
    public static ObjectMatcher? Create(string name, ObjectMatcher matcher) => name switch
    {
        IntrinsicNames.Ceiling => new Ceiling(matcher),
        IntrinsicNames.Floor => new Floor(matcher),
        IntrinsicNames.Round => new Round(matcher),
        _ => null
    };

    private class Ceiling(ObjectMatcher matcher) : ObjectMatcher<T>
    {
        public override bool Match(T value) => matcher.Match(T.ConvertToInteger<long>(T.Ceiling(value)));
    }

    private class Floor(ObjectMatcher matcher) : ObjectMatcher<T>
    {
        public override bool Match(T value) => matcher.Match(T.ConvertToInteger<long>(T.Floor(value)));
    }

    private class Round(ObjectMatcher matcher) : ObjectMatcher<T>
    {
        public override bool Match(T value) => matcher.Match(T.ConvertToInteger<long>(T.Round(value)));
    }
}
