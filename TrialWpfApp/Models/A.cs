#pragma warning disable CS8618

using System.Diagnostics.CodeAnalysis;

namespace TrialWpfApp.Models;

public class A
{
    public int Id { get; set; }
    public string Name { get; set; }
    public B Item { get; set; }
    public B? Nullable { get; set; }
    public bool Flag { get; set; }
    public C[] Points { get; set; }
    public D[] Structs { get; set; }

    public A R1 { get; set; }
    public A[] R2 { get; set; }
}

public class B
{
    public int Number { get; set; }

    public int[] Values { get; set; }
    public string[] Attributes { get; set; }

    public byte U8 { get; set; }
    public sbyte I8 { get; set; }
    public short I16 { get; set; }
    public ushort U16 { get; set; }
    public int I32 { get; set; }
    public uint U32 { get; set; }
    public long I64 { get; set; }
    public ulong U64 { get; set; }
    public float F32 { get; set; }
    public double F64 { get; set; }

    public DateTime D1 { get; set; }
    public DateTimeOffset D2 { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public TimeSpan TimeSpan { get; set; }

    public E E1 { get; set; }
    public F E2 { get; set; }

    public Comparable Comparable { get; set; }
    public Equatable Equatable { get; set; }

    // Item1, Item2 に化けちゃうと思うけども一応対応する？
    // ただ、Item1 とかはフィールドなので、フィールドに対応するかどうか。
    // public (int X, int Y) Tuple { get; set; }

    public KeyValuePair<int, int> KeyValuePair { get; set; }

    public List<D> List { get; set; }
    public Dictionary<string, D> Dictionary { get; set; }

    public string Description { get; set; }
}

public record class C(int X, int Y);
public record struct D(int X, int Y);

public enum E
{
    A,
    B,
    C,
    C1 = C,
}

[Flags]
public enum F
{
    A = 1,
    B = 2,
    C = 4,
    AB = A | B,
    ABC = A | B | C,
}

public readonly struct Comparable(int value) : IComparable<Comparable>, ISpanParsable<Comparable>
{
    public int Value => value;
    public static Comparable Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => new(int.Parse(s, provider));
    public static Comparable Parse(string s, IFormatProvider? provider) => new(int.Parse(s, provider));

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Comparable result)
    {
        if (int.TryParse(s, provider, out var value)) { result = new(value); return true; }
        else { result = default; return false; }
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Comparable result)
    {
        if (int.TryParse(s, provider, out var value)) { result = new(value); return true; }
        else { result = default; return false; }
    }

    public int CompareTo(Comparable other) => value.CompareTo(other.Value);
    public override string ToString() => value.ToString();
}

#pragma warning disable IDE0079
#pragma warning disable CA1067
public readonly struct Equatable(int value) : IEquatable<Equatable>, ISpanParsable<Equatable>
{
    public int Value => value;
    public static Equatable Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => new(int.Parse(s, provider));
    public static Equatable Parse(string s, IFormatProvider? provider) => new(int.Parse(s, provider));

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Equatable result)
    {
        if (int.TryParse(s, provider, out var value)) { result = new(value); return true; }
        else { result = default; return false; }
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Equatable result)
    {
        if (int.TryParse(s, provider, out var value)) { result = new(value); return true; }
        else { result = default; return false; }
    }

    public bool Equals(Equatable other) => value.Equals(other.Value);
    public override string ToString() => value.ToString();
}
