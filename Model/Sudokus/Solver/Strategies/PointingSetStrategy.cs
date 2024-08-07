using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;

namespace Model.Sudokus.Solver.Strategies;

/// <summary>
/// Pointing possibilities is a pattern where a possibility in a mini grid is restrained to a row or column. That means that,
/// wherever this possibility ends up as a solution in that mini grid, it will always remove the possibility from the
/// remaining cells of the row/column.
///
/// Example :
///
/// +-------+-------+-------+
/// | x x x | y y y | y y y |
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
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
/// If a possibility is present in only the x-marked cells in the first mini grid, then it can be removed from all
/// y-marked cells
/// </summary>
public class PointingSetStrategy : SudokuStrategy
{
    public const string OfficialName = "Pointing Set";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    public PointingSetStrategy() : base(OfficialName, Difficulty.Easy, DefaultInstanceHandling){}

    public override void Apply(ISudokuSolverData solverData)
    {
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                for (int number = 1; number <= 9; number++)
                {
                    var ppimg = solverData.MiniGridPositionsAt(miniRow, miniCol, number);
                    if (ppimg.AreAllInSameRow())
                    {
                        int row = ppimg.First().Row;
                        for (int col = 0; col < 9; col++)
                        {
                            if (col / 3 != miniCol) solverData.ChangeBuffer.ProposePossibilityRemoval(number, row, col);
                        }
                        
                        if (solverData.ChangeBuffer.NeedCommit())
                        {
                            solverData.ChangeBuffer.Commit(new PointingPossibilitiesReportBuilder(number, ppimg, Unit.Row));
                            if (StopOnFirstCommit) return;
                        }
                    }
                    else if (ppimg.AreAllInSameColumn())
                    {
                        int col = ppimg.First().Column;
                        for (int row = 0; row < 9; row++)
                        {
                            if (row / 3 != miniRow) solverData.ChangeBuffer.ProposePossibilityRemoval(number, row, col);
                        }

                        if (solverData.ChangeBuffer.NeedCommit())
                        {
                            solverData.ChangeBuffer.Commit(new PointingPossibilitiesReportBuilder(number, ppimg, Unit.Column));
                            if (StopOnFirstCommit) return;
                        }
                    }
                }
            }
        }
    }
}

public class PointingPossibilitiesReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly int _number;
    private readonly IReadOnlyBoxPositions _miniPos;
    private readonly Unit _unit;

    public PointingPossibilitiesReportBuilder(int number, IReadOnlyBoxPositions miniPos, Unit unit)
    {
        _number = number;
        _miniPos = miniPos;
        _unit = unit;
    }
    
    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>(Description(changes), lighter =>
        {
            foreach (var pos in _miniPos)
            {
                lighter.HighlightPossibility(_number, pos.Row, pos.Column, StepColor.Cause1);
            }
            
            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    private string Description(IReadOnlyList<NumericChange> changes)
    {
        var n = _unit switch
        {
            Unit.Row => changes[0].Row,
            Unit.Column => changes[0].Column,
            _ => 0
        };
        return $"Pointing Set in {_unit.ToString().ToLower()} {n + 1} because of box {_miniPos.GetNumber() + 1}";
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}