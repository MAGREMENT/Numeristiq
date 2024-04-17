using System.Collections.Generic;
using Model.Sudokus.Generator.Criterias;

namespace Model.Sudokus.Generator;

public static class CriteriaPool
{
    private static readonly Dictionary<string, GiveCriteria> Pool = new()
    {
        {MinimumRatingCriteria.OfficialName, () => new MinimumRatingCriteria()},
        {MaximumRatingCriteria.OfficialName, () => new MaximumRatingCriteria()},
        {MinimumHardestDifficultyCriteria.OfficialName, () => new MinimumHardestDifficultyCriteria()},
        {MaximumHardestDifficultyCriteria.OfficialName, () => new MaximumHardestDifficultyCriteria()},
        {MustUseStrategyCriteria.OfficialName, () => new MustUseStrategyCriteria()},
        {CantUseStrategyCriteria.OfficialName, () => new CantUseStrategyCriteria()},
    };
    
    public static IEnumerable<string> EnumerateCriterias(string filter)
    {
        var lFilter = filter.ToLower();

        foreach (var name in Pool.Keys)
        {
            if (name.ToLower().Contains(lFilter)) yield return name;
        }
    }

    public static IEnumerable<string> EnumerateCriterias() => Pool.Keys;

    public static EvaluationCriteria? CreateFrom(string s) 
        => Pool.TryGetValue(s, out var giver) ? giver() : null;
}

public delegate EvaluationCriteria GiveCriteria();