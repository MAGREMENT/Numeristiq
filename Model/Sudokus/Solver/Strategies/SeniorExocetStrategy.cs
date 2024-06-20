using Model.Core;
using Model.Sudokus.Solver.Utility.Exocet;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class SeniorExocetStrategy : SudokuStrategy
{
    public const string OfficialName = "Senior Exocet";
    
    public SeniorExocetStrategy() : base(OfficialName, StepDifficulty.Extreme, InstanceHandling.FirstOnly)
    {
    }

    public override void Apply(ISudokuSolverData solverData)
    {
        foreach (var exo in ExocetSearcher.SearchSeniors(solverData))
        {
            foreach (var p in solverData.PossibilitiesAt(exo.Target1).EnumeratePossibilities())
            {
                if(!exo.BaseCandidates.Contains(p)) solverData.ChangeBuffer.ProposePossibilityRemoval(p, exo.Target1);
            }
            
            foreach (var p in solverData.PossibilitiesAt(exo.Target2).EnumeratePossibilities())
            {
                if(!exo.BaseCandidates.Contains(p)) solverData.ChangeBuffer.ProposePossibilityRemoval(p, exo.Target2);
            }

            if (exo.BaseCandidates.Count == 2)
            {
                JuniorExocetStrategy.RemoveAllNonSCells(solverData, exo, exo.ComputeAllCoverHouses());
            }
            
            if(solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer
                   .Commit(new DoubleTargetExocetReportBuilder(exo)) && StopOnFirstPush) return;
        }
    }
}