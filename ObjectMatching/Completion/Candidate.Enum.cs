using CodeCompletion.Completion;
using System.Reflection;
using X = ObjectMatching.Reflection;

namespace ObjectMatching.Completion;

partial class Candidates
{
    private record struct NamaValue(string Name, long Value);

    private static CandidateList GetEnumCandidates(X.TypeInfo type)
    {
        var fields = type.Type.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f => new NamaValue(f.Name, Value: Convert.ToInt64(f.GetRawConstantValue())))
            .ToArray();

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
}
