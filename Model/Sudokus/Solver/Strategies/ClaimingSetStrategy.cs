using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Explanation;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility;

namespace Model.Sudokus.Solver.Strategies;

public class ClaimingSetStrategy : SudokuStrategy
{
    public const string OfficialName = "Claiming Set";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.UnorderedAll;

    public ClaimingSetStrategy() : base(OfficialName, Difficulty.Easy, DefaultInstanceHandling){}

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

                    if (solverData.ChangeBuffer.NeedCommit())
                    {
                        solverData.ChangeBuffer.Commit(new ClaimingSetReportBuilder(row,
                            ppir, number, Unit.Row));
                        if (StopOnFirstCommit) return;
                    }
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

                    if (solverData.ChangeBuffer.NeedCommit())
                    {
                        solverData.ChangeBuffer.Commit(new ClaimingSetReportBuilder(col,
                            ppic, number, Unit.Column));
                        if (StopOnFirstCommit) return;
                    }
                }
            }
        }
    }
}

public class ClaimingSetReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly int _unitNumber;
    private readonly IReadOnlyLinePositions _linePos;
    private readonly int _number;
    private readonly Unit _unit;

    public ClaimingSetReportBuilder(int unitNumber, IReadOnlyLinePositions linePos, int number, Unit unit)
    {
        _unitNumber = unitNumber;
        _linePos = linePos;
        _number = number; 
        _unit = unit;
    }
    
    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        var causes = _linePos.ToCellArray(_unit, _unitNumber);

        var start = new StringExplanationElement($"All the possibilities for {_number} in ");
        var buffer = start.Append(new House(_unit, _unitNumber)).Append(" are in ")
            .Append(new House(Unit.Box, GetBoxNumber())).Append(". Which means that whatever possibility between ")
            .Append(causes[0]);

        for (int i = 1; i < causes.Length; i++)
        {
            buffer = buffer.Append(", ");
            buffer = buffer.Append(causes[i]);
        }

        buffer.Append(" is the solution for ").Append(new House(_unit, _unitNumber))
            .Append($", it will remove the other {_number}'s from ").Append(new House(Unit.Box, GetBoxNumber()));

        return new ChangeReport<ISudokuHighlighter>(Description(), lighter =>
        {
            foreach (var coord in causes)
            {
                lighter.HighlightPossibility(_number, coord.Row, coord.Column, StepColor.Cause1);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        }, start);
    }

    private string Description()
    {
        return $"Claiming Set in box {GetBoxNumber() + 1} because of {_unit.ToString().ToLower()} {_unitNumber + 1}";
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new Clue<ISudokuHighlighter>(lighter =>
            {
                lighter.EncircleHouse(new House(_unit, _unitNumber), StepColor.Cause1);
                lighter.EncircleHouse(new House(Unit.Box, GetBoxNumber()), StepColor.Cause2);
            }, $"This box and that {_unit.ToString().ToLower()} have an interesting intersection");
    }

    private int GetBoxNumber()
    {
        return _unit switch
        {
            Unit.Row => _unitNumber / 3 * 3 + _linePos.First() / 3,
            Unit.Column => _linePos.First() / 3 * 3 + _unitNumber / 3,
            _ => 0
        };
    }
}