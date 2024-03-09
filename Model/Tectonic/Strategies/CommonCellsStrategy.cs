using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Sudoku.Solver;
using Model.Utility;

namespace Model.Tectonic.Strategies;

public class CommonCellsStrategy : TectonicStrategy
{
    public CommonCellsStrategy() : base("Common Cells", StrategyDifficulty.Easy, OnCommitBehavior.WaitForAll)
    {
    }
    
    public override void Apply(IStrategyUser strategyUser)
    {
        List<Cell> buffer = new();
        
        foreach (var zone in strategyUser.Tectonic.Zones)
        {
            for (int n = 1; n <= zone.Count; n++)
            {
                foreach (var cell in zone)
                {
                    if (strategyUser.PossibilitiesAt(cell).Contains(n)) buffer.Add(cell);
                }

                if (buffer.Count == 0) continue;

                foreach (var neighbor in Cells.SharedNeighboringCells(strategyUser.Tectonic, buffer))
                {
                    strategyUser.ChangeBuffer.ProposePossibilityRemoval(n, neighbor);
                }

                strategyUser.ChangeBuffer.Commit(new CommonCellsReportBuilder());
                buffer.Clear();
            }
        }
    }
}

public class CommonCellsReportBuilder : IChangeReportBuilder<IUpdatableTectonicSolvingState>
{
    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, IUpdatableTectonicSolvingState snapshot)
    {
        return ChangeReport.Default(changes);
    }
}