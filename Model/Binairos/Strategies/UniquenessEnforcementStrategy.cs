using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Binairos.Strategies;

public class UniquenessEnforcementStrategy : Strategy<IBinairoSolverData>
{
    public UniquenessEnforcementStrategy() : base("Uniqueness Enforcement", Difficulty.Easy, InstanceHandling.FirstOnly)
    {
    }

    public override void Apply(IBinairoSolverData data)
    {
        var half = data.ColumnCount / 2;
        for (int row = 0; row < data.RowCount; row++)
        {
            var set = data.Binairo.RowSetAt(row);
            if(set.OnesCount != half - 1 || set.TwosCount != half - 1) continue;

            for (int r = 0; r < data.RowCount; r++)
            {
                var set2 = data.Binairo.RowSetAt(row);
                if(set2.GetTotalCount() != data.ColumnCount || !set2.Contains(set)) continue;

                for (int col = 0; col < data.ColumnCount; col++)
                {
                    if(data[row, col] == 0) data.ChangeBuffer.ProposeSolutionAddition(
                        BinairoUtility.Opposite(set2[col]), row, col);
                }
                
                if (data.ChangeBuffer.NeedCommit())
                {
                    data.ChangeBuffer.Commit(new UniquenessEnforcementReportBuilder(row, true, r));
                    if (StopOnFirstCommit) return;
                }
                
                break;
            }
        }
        
        half = data.RowCount / 2;
        for (int col = 0; col < data.ColumnCount; col++)
        {
            var set = data.Binairo.ColumnSetAt(col);
            if(set.OnesCount != half - 1 || set.TwosCount != half - 1) continue;

            for (int c = 0; c < data.ColumnCount; c++)
            {
                var set2 = data.Binairo.ColumnSetAt(c);
                if(set2.GetTotalCount() != data.RowCount || !set2.Contains(set)) continue;

                for (int row = 0; row < data.ColumnCount; row++)
                {
                    if(data[row, col] == 0) data.ChangeBuffer.ProposeSolutionAddition(
                        BinairoUtility.Opposite(set2[col]), row, col);
                }
                
                if (data.ChangeBuffer.NeedCommit())
                {
                    data.ChangeBuffer.Commit(new UniquenessEnforcementReportBuilder(col, false, c));
                    if (StopOnFirstCommit) return;
                }
                
                break;
            }
        }
    }
}

public class UniquenessEnforcementReportBuilder : IChangeReportBuilder<BinaryChange, IBinarySolvingState, IBinairoHighlighter>
{
    private readonly int _unit;
    private readonly bool _isRow;
    private readonly int _otherUnit;

    public UniquenessEnforcementReportBuilder(int unit, bool isRow, int otherUnit)
    {
        _unit = unit;
        _isRow = isRow;
        _otherUnit = otherUnit;
    }

    public ChangeReport<IBinairoHighlighter> BuildReport(IReadOnlyList<BinaryChange> changes, IBinarySolvingState snapshot)
    {
        var u = _isRow ? 'r' : 'c';
        return new ChangeReport<IBinairoHighlighter>($"Uniqueness Enforcement in {u}{_unit} because of {u}{_otherUnit}",
            lighter =>
            {
                if (_isRow)
                {
                    for (int col = 0; col < snapshot.ColumnCount; col++)
                    {
                        if(snapshot[_unit, col] == 0) lighter.HighlightCell(_otherUnit, col, StepColor.Cause1);
                    }
                }
                else
                {
                    for (int row = 0; row < snapshot.RowCount; row++)
                    {
                        if (snapshot[row, _unit] == 0) lighter.HighlightCell(row, _unit, StepColor.Cause1);
                    }
                }

                ChangeReportHelper.HighlightChanges(lighter, changes);
            });
    }

    public Clue<IBinairoHighlighter> BuildClue(IReadOnlyList<BinaryChange> changes, IBinarySolvingState snapshot)
    {
        return Clue<IBinairoHighlighter>.Default();
    }
}