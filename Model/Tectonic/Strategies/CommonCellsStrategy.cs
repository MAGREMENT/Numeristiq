using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudoku.Solver;
using Model.Utility;
using Model.Utility.BitSets;

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
                
                buffer.Clear();
            }

            strategyUser.ChangeBuffer.Commit(new CommonCellsReportBuilder(zone));
        }
    }
}

public class CommonCellsReportBuilder : IChangeReportBuilder<ITectonicSolvingState, ITectonicHighlighter>
{
    private readonly Zone _zone;

    public CommonCellsReportBuilder(Zone zone)
    {
        _zone = zone;
    }

    public ChangeReport<ITectonicHighlighter> Build(IReadOnlyList<SolverProgress> changes, ITectonicSolvingState snapshot)
    {
        return new ChangeReport<ITectonicHighlighter>("", lighter =>
        {
            var done = new ReadOnlyBitSet16();

            foreach (var change in changes)
            {
                if(done.Contains(change.Number)) continue;

                done += change.Number;

                foreach (var cell in _zone)
                {
                    if(snapshot.PossibilitiesAt(cell).Contains(change.Number)) 
                        lighter.HighlightPossibility(cell, change.Number, ChangeColoration.CauseOffOne);
                }
            }
            
            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
}