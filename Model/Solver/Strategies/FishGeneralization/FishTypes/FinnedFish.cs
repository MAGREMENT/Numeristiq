using System.Collections.Generic;
using Model.Solver.Position;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.Strategies.FishGeneralization.FishTypes;

public class FinnedFish : IFishType
{
    public string Name => "Finned Fish";
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

        foreach (var coveredGrid in gp.PossibleCoveredGrids(combination.Length, 3, baseSet,
                     UnitMethods.All))
        {
            var gpOfCoverSet = new GridPositions();
            foreach (var set in coveredGrid.CoverHouses)
            {
                UnitMethods.GetMethods(set.Unit).Fill(gpOfCoverSet, set.Number);
            }
            var diff = gpOfCoverSet.Difference(gp);

            if (coveredGrid.Remaining.Count == 0)
            {
                foreach (var cell in diff)
                {
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(number, cell);
                } 
            }
            else
            {
                var asArray = coveredGrid.Remaining.ToArray();
                foreach (var ssc in Cells.SharedSeenCells(asArray))
                {
                    if (!diff.Peek(ssc)) continue;
                    
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(number, ssc);
                }
            }

            if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(strategy,
                    new FishReportBuilder(baseSet, coveredGrid.CoverHouses, number, gp, coveredGrid.Remaining)) &&
                strategy.OnCommitBehavior == OnCommitBehavior.Return) return true;
        }
        
        return false;
    }
}