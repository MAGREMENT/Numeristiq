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
    
    public override void Apply(ITectonicSolverData data)
    {
        List<Cell> buffer = new();
        
        foreach (var zone in data.Tectonic.Zones)
        {
            for (int n = 1; n <= zone.Count; n++)
            {
                foreach (var cell in zone)
                {
                    if (data.PossibilitiesAt(cell).Contains(n)) buffer.Add(cell);
                }

                if (buffer.Count == 0) continue;

                foreach (var neighbor in TectonicCellUtility.SharedNeighboringCells(data.Tectonic, buffer))
                {
                    data.ChangeBuffer.ProposePossibilityRemoval(n, neighbor);
                }
                
                buffer.Clear();
            }

            data.ChangeBuffer.Commit(new ZoneInteractionReportBuilder(zone));
        }
    }
}

public class ZoneInteractionReportBuilder : IChangeReportBuilder<NumericChange, INumericSolvingState, ITectonicHighlighter>
{
    private readonly IZone _zone;

    public ZoneInteractionReportBuilder(IZone zone)
    {
        _zone = zone;
    }

    public ChangeReport<ITectonicHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, INumericSolvingState snapshot)
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

    public Clue<ITectonicHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, INumericSolvingState snapshot)
    {
        return Clue<ITectonicHighlighter>.Default();
    }
}