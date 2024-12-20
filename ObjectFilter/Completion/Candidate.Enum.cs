using CodeCompletion.Completion;
using System.Reflection;
using X = ObjectFilter.Reflection;

namespace ObjectFilter.Completion;

partial class Candidates
{
    private record struct NamaValue(string Name, long Value);

    private static CandidateList GetEnumCandidates(X.TypeInfo type)
    {
        var fields = type.Type.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f => new NamaValue(f.Name, Value: Convert.ToInt64(f.GetRawConstantValue())))
            .ToArray();

        if (IsFlagsEnum(type.Type))
        {
            fields = [.. fields, .. GetDisjunctiveValues(fields)];
        }

        var candidates = new List<Candidate>();

        // メンバー名: 値
        foreach (var (name, value) in fields)
        {
            candidates.Add(new(name, value.ToString()));
        }

        // 値: メンバー名
        foreach (var g in fields.GroupBy(f => f.Value).OrderBy(g => g.Key))
        {
            candidates.Add(new(g.Key.ToString()!, string.Join(", ", g.Select(f => f.Name))));
        }

        return new(type.Description, candidates);
    }

    private static bool IsFlagsEnum(Type t) => t.GetCustomAttribute<FlagsAttribute>() is not null;

    private static List<NamaValue> GetDisjunctiveValues(IEnumerable<NamaValue> fields)
    {
        var values = fields.Select(x => x.Value).Distinct();
        var pow2Values = fields.DistinctBy(x => x.Value).Where(x => System.Numerics.BitOperations.IsPow2(x.Value)).ToArray();

        var pow = 1 << pow2Values.Length;
        var powerSet = new List<NamaValue>();
        var names = new List<string>();
        for (int n = 1; n < pow; n++)
        {
            // 2のべきだと、後ろの if (Contains) continue; のところが必ず true になるはずなので計算するだけ無駄。
            if (System.Numerics.BitOperations.IsPow2(n)) continue;

            // enum X { A = 1, B = 2, C = 4 } みたいなのから A|B, A|C, B|C, A|B|C を作る。
            // Value は |= で、Name は , で Join。
            // (ここは A, B, C も作るコードになってるけど、↑の IsPow2 ではじかれる。)
            names.Clear();
            long value = 0;
            var bits = n;
            for (int i = 0; i < pow2Values.Length; i++)
            {
                if ((bits & 1) != 0)
                {
                    var x = pow2Values[i];
                    names.Add(x.Name);
                    value |= x.Value;
                }
                bits >>= 1;
            }

            // Ab = A | B みたいな値が元々定義されてたら、A | B は除外する。Ab だけで十分。
            if (values.Contains(value)) continue;

            // A | B みたいな enum 値を ToString すると A,B になる。
            // そのままだと , で Split されちゃうんで "" でくくる。
            powerSet.Add(new('"' + string.Join(",", names) + '"', value));
        }

        return powerSet;
    }
}
