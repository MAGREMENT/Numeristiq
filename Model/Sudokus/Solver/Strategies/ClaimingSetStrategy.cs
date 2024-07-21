using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;

namespace Model.Sudokus.Solver.Strategies;

/// <summary>
/// Box line reduction is a pattern where a possibility in a row/column is restrained to a single mini grid. That means that,
/// wherever this possibility ends up as a solution in that row or column, it will always remove the possibility from the
/// remaining cells of the mini grid.
///
/// Example :
///
/// +-------+-------+-------+
/// | x x x | . . . | . . . |
/// | y y y | . . . | . . . |
/// | y y y | . . . | . . . |
/// +-------+-------+-------+
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// +-------+-------+-------+
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// +-------+-------+-------+
///
/// If a possibility is present in only the x-marked cells in the first row, then it can be removed from all
/// y-marked cells
/// </summary>
public class ClaimingSetStrategy : SudokuStrategy
{
    public const string OfficialName = "Claiming Set";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    public ClaimingSetStrategy() : base(OfficialName, StepDifficulty.Easy, DefaultInstanceHandling){}

    public override void Apply(ISudokuSolverData solverData)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppir = solverData.RowPositionsAt(row, number);
                if (ppir.AreAllInSameMiniGrid())
                {
                    int startRow = row / 3 * 3;
                    int startCol = ppir.First() / 3 * 3;

                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            int removedFromRow = startRow + r;
                            int removeFromCol = startCol + c;

                            if (removedFromRow != row)
                                solverData.ChangeBuffer
                                    .ProposePossibilityRemoval(number, removedFromRow, removeFromCol);
                        }
                    }

                    if (solverData.ChangeBuffer.Commit( new BoxLineReductionReportBuilder(row,
                            ppir, number, Unit.Row)) && StopOnFirstPush) return;
                }
            }

            for (int col = 0; col < 9; col++)
            {

                var ppic = solverData.ColumnPositionsAt(col, number);
                if (ppic.AreAllInSameMiniGrid())
                {
                    int startRow = ppic.First() / 3 * 3;
                    int startCol = col / 3 * 3;

                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            int removedFromRow = startRow + r;
                            int removedFromCol = startCol + c;

                            if (removedFromCol != col)
                                solverData.ChangeBuffer
                                    .ProposePossibilityRemoval(number, removedFromRow, removedFromCol);
                        }
                    }

                    if(solverData.ChangeBuffer.Commit( new BoxLineReductionReportBuilder(col,
                           ppic, number, Unit.Column)) && StopOnFirstPush) return;
                }
            }
        }
    }
}

public class BoxLineReductionReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly int _unitNumber;
    private readonly IReadOnlyLinePositions _linePos;
    private readonly int _number;
    private readonly Unit _unit;

    public BoxLineReductionReportBuilder(int unitNumber, IReadOnlyLinePositions linePos, int number, Unit unit)
    {
        _unitNumber = unitNumber;
        _linePos = linePos;
        _number = number; 
        _unit = unit;
    }
    
    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        var causes = _linePos.ToCellArray(_unit, _unitNumber);

        return new ChangeReport<ISudokuHighlighter>(Description(), lighter =>
        {
            foreach (var coord in causes)
            {
                lighter.HighlightPossibility(_number, coord.Row, coord.Column, StepColor.Cause1);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    private string Description()
    {
        var box = _unit switch
        {
            Unit.Row => _unitNumber / 3 * 3 + _linePos.First() / 3 + 1,
            Unit.Column => _linePos.First() / 3 * 3 + _unitNumber / 3 + 1,
            _ => 0
        };
        return $"Claiming Set in box {box} because of {_unit.ToString().ToLower()} {_unitNumber + 1}";
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}