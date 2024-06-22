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

    public TwoStringKiteStrategy() : base(OfficialName, StepDifficulty.Hard, DefaultInstanceHandling)
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

                            foreach (var c in SudokuCellUtility.SharedSeenCells(rOther, cOther))
                            {
                                solverData.ChangeBuffer.ProposePossibilityRemoval(number, c);
                            }

                            if (solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                                    new TwoStringKiteReportBuilder(number, rCommon, cCommon, rOther,
                                        cOther)) && StopOnFirstPush) return;
                        }
                    }
                }
            }
        }
    }
}

public class TwoStringKiteReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly int _possibility;
    private readonly Cell _inCommon1;
    private readonly Cell _inCommon2;
    private readonly Cell _other1;
    private readonly Cell _other2;

    public TwoStringKiteReportBuilder(int possibility, Cell inCommon1, Cell inCommon2, Cell other1, Cell other2)
    {
        _possibility = possibility;
        _inCommon1 = inCommon1;
        _inCommon2 = inCommon2;
        _other1 = other1;
        _other2 = other2;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            lighter.HighlightPossibility(_possibility, _inCommon1.Row, _inCommon1.Column, ChangeColoration.CauseOffOne);
            lighter.HighlightPossibility(_possibility, _inCommon2.Row, _inCommon2.Column, ChangeColoration.CauseOffOne);
            lighter.HighlightPossibility(_possibility, _other1.Row, _other1.Column, ChangeColoration.CauseOffOne);
            lighter.HighlightPossibility(_possibility, _other2.Row, _other2.Column, ChangeColoration.CauseOffOne);
            
            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}