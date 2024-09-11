using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies;

public class TwoStringKiteStrategy : SudokuStrategy
{
    public const string OfficialName = "Two-String Kite";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public TwoStringKiteStrategy() : base(OfficialName, Difficulty.Hard, DefaultInstanceHandling)
    {
    }

    
    public override void Apply(ISudokuSolverData solverData)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var rowPositions = solverData.RowPositionsAt(row, number);
                if (rowPositions.Count != 2 || rowPositions.AreAllInSameMiniGrid()) continue;

                for (int col = 0; col < 9; col++)
                {
                    var colPositions = solverData.ColumnPositionsAt(col, number);
                    if (colPositions.Count != 2 || colPositions.AreAllInSameMiniGrid()) continue;

                    var rowCell = rowPositions.ToCellArray(Unit.Row, row);
                    var colCell = colPositions.ToCellArray(Unit.Column, col);

                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            var rCommon = rowCell[i];
                            var cCommon = colCell[j];
                            if (rCommon == cCommon) continue;
                            if(rCommon.Row / 3 != cCommon.Row / 3 || rCommon.Column / 3 != cCommon.Column / 3) continue;

                            var rOther = rowCell[(i + 1) % 2];
                            var cOther = colCell[(j + 1) % 2];

                            foreach (var c in SudokuUtility.SharedSeenCells(rOther, cOther))
                            {
                                solverData.ChangeBuffer.ProposePossibilityRemoval(number, c);
                            }

                            if (solverData.ChangeBuffer.NeedCommit())
                            {
                                solverData.ChangeBuffer.Commit(new TwoStringKiteReportBuilder(number, rCommon, cCommon, rOther,
                                    cOther));
                                if (StopOnFirstCommit) return;
                            }
                        }
                    }
                }
            }
        }
    }
}

public class TwoStringKiteReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly int _possibility;
    private readonly Cell _rCommon;
    private readonly Cell _cCommon;
    private readonly Cell _rOther;
    private readonly Cell _cOther;

    public TwoStringKiteReportBuilder(int possibility, Cell rCommon, Cell cCommon, Cell rOther, Cell cOther)
    {
        _possibility = possibility;
        _rCommon = rCommon;
        _cCommon = cCommon;
        _rOther = rOther;
        _cOther = cOther;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>($"Two-String Kite for {_possibility} in r{_rCommon.Row + 1}" +
                                                    $"and c{_cCommon.Column + 1} with the help of b{_rCommon.GetBox() + 1}", lighter =>
        {
            lighter.HighlightPossibility(_possibility, _rCommon.Row, _rCommon.Column, StepColor.Cause1);
            lighter.HighlightPossibility(_possibility, _cCommon.Row, _cCommon.Column, StepColor.Cause1);
            lighter.HighlightPossibility(_possibility, _rOther.Row, _rOther.Column, StepColor.Cause1);
            lighter.HighlightPossibility(_possibility, _cOther.Row, _cOther.Column, StepColor.Cause1);
            
            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}