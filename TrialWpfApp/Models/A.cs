#pragma warning disable CS8618

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

    public E E1 { get; set; }
    public F E2 { get; set; }

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
