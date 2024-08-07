using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.Tectonics.Solver.Strategies;

public class HiddenDoubleStrategy : Strategy<ITectonicSolverData>
{
    public HiddenDoubleStrategy() : base("Hidden Double", Difficulty.Easy, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(ITectonicSolverData data)
    {
        Dictionary<ReadOnlyBitSet8, int> dic = new();
        for (int i = 0; i < data.Tectonic.Zones.Count; i++)
        {
            var z = data.Tectonic.Zones[i];
            for (int n = 1; n <= z.Count; n++)
            {
                var p = data.ZonePositionsFor(i, n);
                if(p.Count != 2) continue;

                if (dic.TryGetValue(p, out var other))
                {
                    foreach (var c in p.EnumeratePositions(z))
                    {
                        var cell = z[c];
                        foreach (var possibility in data.PossibilitiesAt(cell).EnumeratePossibilities())
                        {
                            if (possibility == n || possibility == other) continue;

                            data.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
                        }
                    }

                    if (data.ChangeBuffer.NeedCommit())
                    {
                        data.ChangeBuffer.Commit(new HiddenDoubleReportBuilder(z, p, n, other));
                        if (StopOnFirstCommit) return;
                    }
                }
                else dic[p] = n;
            }
            
            dic.Clear();
        }
    }
}

public class HiddenDoubleReportBuilder : IChangeReportBuilder<NumericChange, INumericSolvingState, ITectonicHighlighter>
{
    private readonly IZone _zone;
    private readonly ReadOnlyBitSet8 _p;
    private readonly int _n1;
    private readonly int _n2;

    public HiddenDoubleReportBuilder(IZone zone, ReadOnlyBitSet8 p, int n1, int n2)
    {
        _zone = zone;
        _p = p;
        _n1 = n1;
        _n2 = n2;
    }

    public ChangeReport<ITectonicHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, INumericSolvingState snapshot)
    {
        var cells = new Cell[2];
        var cursor = 0;
        foreach (var c in _p.EnumeratePositions(_zone))
        {
            cells[cursor++] = _zone[c];
        }

        return new ChangeReport<ITectonicHighlighter>($"Hidden Double in {cells.ToStringSequence(" and ")}" +
                                                      $"for {_n1} and {_n2}", lighter =>
        {
            foreach (var cell in cells)
            {
                lighter.HighlightPossibility(cell, _n1, StepColor.Cause1);
                lighter.HighlightPossibility(cell, _n2, StepColor.Cause1);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    public Clue<ITectonicHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, INumericSolvingState snapshot)
    {
        return Clue<ITectonicHighlighter>.Default();
    }
}