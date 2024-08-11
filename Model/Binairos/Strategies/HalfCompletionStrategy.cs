using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Binairos.Strategies;

public class HalfCompletionStrategy : Strategy<IBinairoSolverData>
{
    public HalfCompletionStrategy() : base("Half Completion", Difficulty.Basic, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(IBinairoSolverData data)
    {
        var objective = data.RowCount / 2;
        for (int row = 0; row < data.RowCount; row++)
        {
            var set = data.Binairo.RowSetAt(row);
            int toAdd = 0;
            if (set.OnesCount == objective)
            {
                if (set.TwosCount != objective) toAdd = 2;
            }else if (set.TwosCount == objective) toAdd = 1;
            
            if(toAdd == 0) continue;
            
            for (int col = 0; col < data.ColumnCount; col++)
            {
                data.ChangeBuffer.ProposeSolutionAddition(toAdd, row, col);
            }

            if (data.ChangeBuffer.NeedCommit())
            {
                data.ChangeBuffer.Commit(new HalfCompletionReportBuilder(row, toAdd, true));
                if(StopOnFirstCommit) return;
            }
        }

        objective = data.ColumnCount / 2;
        for (int col = 0; col < data.ColumnCount; col++)
        {
            var set = data.Binairo.ColumnSetAt(col);
            int toAdd = 0;
            if (set.OnesCount == objective)
            {
                if (set.TwosCount != objective) toAdd = 2;
            }else if (set.TwosCount == objective) toAdd = 1;
            
            if(toAdd == 0) continue;
            
            for (int row = 0; row < data.ColumnCount; row++)
            {
                data.ChangeBuffer.ProposeSolutionAddition(toAdd, row, col);
            }

            if (data.ChangeBuffer.NeedCommit())
            {
                data.ChangeBuffer.Commit(new HalfCompletionReportBuilder(col, toAdd, false));
                if(StopOnFirstCommit) return;
            }
        }
    }
}

public class HalfCompletionReportBuilder : IChangeReportBuilder<BinaryChange, IBinarySolvingState, IBinairoHighlighter>
{
    private readonly int _unit;
    private readonly int _number;
    private readonly bool _isRow;

    public HalfCompletionReportBuilder(int unit, int number, bool isRow)
    {
        _unit = unit;
        _number = number;
        _isRow = isRow;
    }

    public ChangeReport<IBinairoHighlighter> BuildReport(IReadOnlyList<BinaryChange> changes, IBinarySolvingState snapshot)
    {
        var u = _isRow ? 'r' : 'c';
        return new ChangeReport<IBinairoHighlighter>($"Half Completion in {u}{_unit} for {_number - 1}", lighter =>
        {
            if (_isRow)
            {
                for (int col = 0; col < snapshot.ColumnCount; col++)
                {
                    if (snapshot[_unit, col] == _number) lighter.HighlightCell(_unit, col, StepColor.Cause1);
                }
            }
            else
            {
                for (int row = 0; row < snapshot.RowCount; row++)
                {
                    if (snapshot[row, _unit] == _number) lighter.HighlightCell(row, _unit, StepColor.Cause1);
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