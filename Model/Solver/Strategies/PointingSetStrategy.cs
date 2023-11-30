using System;
using System.Collections.Generic;
using Global.Enums;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Position;

namespace Model.Solver.Strategies;

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
public class PointingSetStrategy : AbstractStrategy
{
    public const string OfficialName = "Pointing Set";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.WaitForAll;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public PointingSetStrategy() : base(OfficialName, StrategyDifficulty.Easy, DefaultBehavior){}

    public override void Apply(IStrategyManager strategyManager)
    {
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                for (int number = 1; number <= 9; number++)
                {
                    var ppimg = strategyManager.MiniGridPositionsAt(miniRow, miniCol, number);
                    if (ppimg.AreAllInSameRow())
                    {
                        int row = ppimg.First().Row;
                        for (int col = 0; col < 9; col++)
                        {
                            if (col / 3 != miniCol) strategyManager.ChangeBuffer.ProposePossibilityRemoval(number, row, col);
                        }
                        
                        if(strategyManager.ChangeBuffer.Commit(this,
                            new PointingPossibilitiesReportBuilder(number, ppimg)) &&
                                OnCommitBehavior == OnCommitBehavior.Return) return;
                    }
                    else if (ppimg.AreAllInSameColumn())
                    {
                        int col = ppimg.First().Column;
                        for (int row = 0; row < 9; row++)
                        {
                            if (row / 3 != miniRow) strategyManager.ChangeBuffer.ProposePossibilityRemoval(number, row, col);
                        }

                        if (strategyManager.ChangeBuffer.Commit(this,
                                new PointingPossibilitiesReportBuilder(number, ppimg)) &&
                                    OnCommitBehavior == OnCommitBehavior.Return) return;
                    }
                }
            }
        }
    }
}

public class PointingPossibilitiesReportBuilder : IChangeReportBuilder
{
    private readonly int _number;
    private readonly IReadOnlyMiniGridPositions _miniPos;

    public PointingPossibilitiesReportBuilder(int number, IReadOnlyMiniGridPositions miniPos)
    {
        _number = number;
        _miniPos = miniPos;
    }
    
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(changes), lighter =>
        {
            foreach (var pos in _miniPos)
            {
                lighter.HighlightPossibility(_number, pos.Row, pos.Column, ChangeColoration.CauseOffOne);
            }
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation(List<SolverChange> changes)
    {
        var firstChange = changes[0];
        var firstMini = _miniPos.First();
        int lineNumber;
        Unit unit;

        if (firstChange.Row == firstMini.Row)
        {
            lineNumber = firstChange.Row;
            unit = Unit.Row;
        }
        else if (firstChange.Column == firstMini.Column)
        {
            lineNumber = firstChange.Column;
            unit = Unit.Column;
        }
        else throw new Exception("Error while backtracking pointing possibilities");

        return $"{_number} is present only in the cells {_miniPos} in mini grid {_miniPos.MiniGridNumber() + 1}, so" +
               $" it can be removed from any other cells in {unit.ToString().ToLower()} {lineNumber}";
    }
}