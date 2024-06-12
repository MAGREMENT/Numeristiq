using System.Collections.Generic;
using Model.Core;
using Model.Sudokus.Generator.Criterias;

namespace Model.Sudokus.Generator;

public static class CriteriaPool
{
    private static readonly Dictionary<string, GiveCriteria> Pool = new()
    {
        {MinimumRatingCriteria.OfficialName, _ => new MinimumRatingCriteria()},
        {MaximumRatingCriteria.OfficialName, _ => new MaximumRatingCriteria()},
        {MinimumHardestDifficultyCriteria.OfficialName, _ => new MinimumHardestDifficultyCriteria()},
        {MaximumHardestDifficultyCriteria.OfficialName, _ => new MaximumHardestDifficultyCriteria()},
        {MustUseStrategyCriteria.OfficialName, s => new MustUseStrategyCriteria(s)},
        {CantUseStrategyCriteria.OfficialName, s => new CantUseStrategyCriteria(s)},
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

    public static EvaluationCriteria? CreateFrom(string s, IReadOnlyList<string> usedStrategies) 
        => Pool.TryGetValue(s, out var giver) ? giver(usedStrategies) : null;
}

public delegate EvaluationCriteria GiveCriteria(IReadOnlyList<string> usedStrategies);