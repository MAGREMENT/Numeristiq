using Model.Sudokus.Solver.Utility.Exocet;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class SeniorExocetStrategy : SudokuStrategy
{
    public const string OfficialName = "Senior Exocet";
    
    public SeniorExocetStrategy() : base(OfficialName, StepDifficulty.Extreme, InstanceHandling.FirstOnly)
    {
    }

    public override void Apply(ISudokuStrategyUser strategyUser)
    {
        foreach (var exo in ExocetSearcher.SearchSeniors(strategyUser))
        {
            foreach (var p in strategyUser.PossibilitiesAt(exo.Target1).EnumeratePossibilities())
            {
                if(!exo.BaseCandidates.Contains(p)) strategyUser.ChangeBuffer.ProposePossibilityRemoval(p, exo.Target1);
            }
            
            foreach (var p in strategyUser.PossibilitiesAt(exo.Target2).EnumeratePossibilities())
            {
                if(!exo.BaseCandidates.Contains(p)) strategyUser.ChangeBuffer.ProposePossibilityRemoval(p, exo.Target2);
            }

            if (exo.BaseCandidates.Count == 2)
            {
                JuniorExocetStrategy.RemoveAllNonSCells(strategyUser, exo, exo.ComputeAllCoverHouses());
            }
            
            if(strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer
                   .Commit(new DoubleTargetExocetReportBuilder(exo)) && StopOnFirstPush) return;
        }
    }
}