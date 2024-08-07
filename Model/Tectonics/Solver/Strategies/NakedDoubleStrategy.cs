using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.Tectonics.Solver.Strategies;

public class NakedDoubleStrategy : Strategy<ITectonicSolverData>
{
    public NakedDoubleStrategy() : base("Naked Double", Difficulty.Easy, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(ITectonicSolverData data)
    {
        Dictionary<ReadOnlyBitSet8, Cell> dic = new();
        foreach (var zone in data.Tectonic.Zones)
        {
            foreach (var cell in zone)
            {
                var p = data.PossibilitiesAt(cell);
                if(p.Count != 2) continue;

                if (dic.TryGetValue(p, out var other))
                {
                    foreach (var c in zone)
                    {
                        if(c == cell || c == other) continue;

                        foreach (var toRemove in p.EnumeratePossibilities())
                        {
                            data.ChangeBuffer.ProposePossibilityRemoval(toRemove, c);
                        }
                    }

                    if (data.ChangeBuffer.NeedCommit() && data.ChangeBuffer.Commit(new NakedDoubleReportBuilder(p,
                            cell, other)) && StopOnFirstCommit) return;
                }
                else dic[p] = cell;
            }
            
            dic.Clear();
        }
    }
}

public class NakedDoubleReportBuilder : IChangeReportBuilder<NumericChange, INumericSolvingState, ITectonicHighlighter>
{
    private readonly ReadOnlyBitSet8 _p;
    private readonly Cell _c1;
    private readonly Cell _c2;

    public NakedDoubleReportBuilder(ReadOnlyBitSet8 p, Cell c1, Cell c2)
    {
        _p = p;
        _c1 = c1;
        _c2 = c2;
    }

    public ChangeReport<ITectonicHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, INumericSolvingState snapshot)
    {
        return new ChangeReport<ITectonicHighlighter>($"Naked Double in {_c1} and {_c2} for" +
                                                      $" {_p.EnumeratePossibilities().ToStringSequence(" and ")}",
            lighter =>
            {
                foreach (var p in _p.EnumeratePossibilities())
                {
                    lighter.HighlightPossibility(_c1, p, StepColor.Cause1);
                    lighter.HighlightPossibility(_c2, p, StepColor.Cause1);
                }

                ChangeReportHelper.HighlightChanges(lighter, changes);
            });
    }

    public Clue<ITectonicHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, INumericSolvingState snapshot)
    {
        return Clue<ITectonicHighlighter>.Default();
    }
}