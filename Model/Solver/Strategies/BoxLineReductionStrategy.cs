using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Position;

namespace Model.Solver.Strategies;

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
public class BoxLineReductionStrategy : AbstractStrategy
{
    public const string OfficialName = "Box-Line Reduction";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public BoxLineReductionStrategy() : base(OfficialName, StrategyDifficulty.Easy, DefaultBehavior){}

    public override void Apply(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppir = strategyManager.RowPositionsAt(row, number);
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
                                strategyManager.ChangeBuffer
                                    .ProposePossibilityRemoval(number, removedFromRow, removeFromCol);
                        }
                    }

                    if (strategyManager.ChangeBuffer.Commit(this, new BoxLineReductionReportBuilder(row,
                            ppir, number, Unit.Row)) && OnCommitBehavior == OnCommitBehavior.Return) return;
                }
            }

            for (int col = 0; col < 9; col++)
            {

                var ppic = strategyManager.ColumnPositionsAt(col, number);
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
                                strategyManager.ChangeBuffer
                                    .ProposePossibilityRemoval(number, removedFromRow, removedFromCol);
                        }
                    }

                    if(strategyManager.ChangeBuffer.Commit(this, new BoxLineReductionReportBuilder(col,
                           ppic, number, Unit.Column)) && OnCommitBehavior == OnCommitBehavior.Return) return;
                }
            }
        }
    }
}

public class BoxLineReductionReportBuilder : IChangeReportBuilder
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
    
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        List<Cell> causes = new();
        switch (_unit)
        {
            case Unit.Row :
                foreach (var col in _linePos)
                {
                    causes.Add(new Cell(_unitNumber, col));
                }
                break;
            case Unit.Column :
                foreach (var row in _linePos)
                {
                    causes.Add(new Cell(row, _unitNumber));
                }
                break;
        }

        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(changes), lighter =>
        {
            foreach (var coord in causes)
            {
                lighter.HighlightPossibility(_number, coord.Row, coord.Col, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation(IReadOnlyList<SolverChange> changes)
    {
        var first = changes[0];
        var miniGirdNumber = first.Row / 3 * 3 + first.Column / 3 + 1;
        return $"{_number} is present only in the cells {_linePos.ToString(_unit, _unitNumber)} in mini" +
               $" {_unit.ToString().ToLower()} {_unitNumber}, so it can be removed from any other cells in" +
               $" mini grid {miniGirdNumber}";
    }
}