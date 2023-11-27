using System.Collections.Generic;
using Model.Solver.Position;

namespace Model.Solver.Strategies.FishGeneralization.FishTypes;

public class BasicFish : IFishType
{
    public string Name => "Fish";

    public bool TryFind(IStrategyManager strategyManager, int number, int[] combination, IStrategy strategy)
    {
        var gp = new GridPositions();
        HashSet<CoverHouse> baseSet = new();
        
        foreach (var n in combination)
        {
            var house = FishGeneralization.CoverHouses[n];
            var methods = UnitMethods.GetMethods(house.Unit);

            if (methods.Count(gp, house.Number) > 0) return false;
            methods.Fill(gp, house.Number);
            baseSet.Add(house);
        }

        gp = gp.And(strategyManager.PositionsFor(number));

        foreach (var coverSet in gp.PossibleCoverHouses(combination.Length, baseSet,
                     UnitMethods.AllUnitMethods))
        {
            var gpOfCoverSet = new GridPositions();
            foreach (var set in coverSet)
            {
                UnitMethods.GetMethods(set.Unit).Fill(gpOfCoverSet, set.Number);
            }
            var diff = gpOfCoverSet.Difference(gp);
            
            foreach (var cell in diff)
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(number, cell);
            }
            
            if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(strategy,
                    new FishReportBuilder(baseSet, coverSet, number, gp)) &&
                        strategy.OnCommitBehavior == OnCommitBehavior.Return) return true;
        }
        
        return false;
    }
}