namespace TrialWpfApp.Models;

internal class SampleData
{
    public static readonly A[] Data =
    [
        new() { Id = 1, Name = "a1" },
        new() { Id = 2, Name = "a2a" },
        new() { Id = 3, Name = "a3aa" },
        new() { Id = 4, Name = "a4aaa", Item = new() { Number = -1, F32 = 0.8f } },
        new() { Id = 5, Name = "a5", Item = new() { Number = 1, F32 = 1.1f } },
        new() { Id = 6, Name = "a6", Item = new() { F32 = 2.3f } },
        new() { Id = 7, Name = "a7", Item = new() { F32 = 3.6f } },
        new() { Id = 8, Name = "a8", Nullable = new() },
        new() { Id = 9, Name = "a9", Flag = true },
        new() { Id = 10, Name = "b" },
        new() { Id = 11, Name = "b1" },
        new() { Id = 12, Name = "b2b" },
        new() { Id = 13, Name = "b3bb" },
        new() { Id = 14, Name = "b4bbb" },
        new() { Id = 15, Name = "b5bbbb" },
        new() { Id = 16, Name = "b6" },
        new() { Id = 17, Name = "b7" },
        new() { Id = 18, Name = "b8", Nullable = new() },
        new() { Id = 19, Name = "b9", Flag = true },
        new() { Id = 20, Points = []},
        new() { Id = 21, Points = [new(1,2)]},
        new() { Id = 22, Points = [new(1,0), new(0,1), new(1,1)]},
        new() { Id = 23, Points = [new(-1, 0)]},
        new() { Id = 24, Points = [new(-5, 0), new(0, -5), new(5, 0), new(0, 5)]},
        new() { Id = 25, Points = [new(0, 0), new(0, 1), new(0, 2), new(0, 3), new(0, 4), new(0, 5)] },
        new() { Id = 26, Structs = [new(1,2)]},
        new() { Id = 27, Structs = [new(1,0), new(0,1), new(1,1)]},
        new() { Id = 28, Structs = [new(-1, 0)]},
        new() { Id = 29, Structs = [new(-5, 0), new(0, -5), new(5, 0), new(0, 5)]},
    ];
}
