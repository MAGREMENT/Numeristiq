using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonics.Solver.Strategies;

public class ZoneInteractionStrategy : Strategy<ITectonicSolverData>
{
    public ZoneInteractionStrategy() : base("Zone Interaction", StepDifficulty.Easy, InstanceHandling.UnorderedAll)
    {
    }
    
    public override void Apply(ITectonicSolverData solverData)
    {
        List<Cell> buffer = new();
        
        foreach (var zone in solverData.Tectonic.Zones)
        {
            for (int n = 1; n <= zone.Count; n++)
            {
                foreach (var cell in zone)
                {
                    if (solverData.PossibilitiesAt(cell).Contains(n)) buffer.Add(cell);
                }

                if (buffer.Count == 0) continue;

                foreach (var neighbor in TectonicCellUtility.SharedNeighboringCells(solverData.Tectonic, buffer))
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(n, neighbor);
                }
                
                buffer.Clear();
            }

            solverData.ChangeBuffer.Commit(new ZoneInteractionReportBuilder(zone));
        }
    }
}

public class ZoneInteractionReportBuilder : IChangeReportBuilder<ITectonicSolvingState, ITectonicHighlighter>
{
    private readonly IZone _zone;

    public ZoneInteractionReportBuilder(IZone zone)
    {
        _zone = zone;
    }

    public ChangeReport<ITectonicHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, ITectonicSolvingState snapshot)
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

    public Clue<ITectonicHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, ITectonicSolvingState snapshot)
    {
        return Clue<ITectonicHighlighter>.Default();
    }
}