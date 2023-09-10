using System.Collections.Generic;
using System.Linq;
using Model.Changes;
using Model.Positions;
using Model.Solver;

namespace Model.Strategies;

public class PointingPossibilitiesStrategy : IStrategy
{
    public string Name { get; } = "Pointing possibilities";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Easy;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                for (int number = 1; number <= 9; number++)
                {
                    var ppimg = strategyManager.MiniGridPositions(miniRow, miniCol, number);
                    if (ppimg.AreAllInSameRow())
                    {
                        int row = ppimg.First().Row;
                        for (int col = 0; col < 9; col++)
                        {
                            if (col / 3 != miniCol) strategyManager.ChangeBuffer.AddPossibilityToRemove(number, row, col);
                        }
                        
                        strategyManager.ChangeBuffer.Push(this,
                            new PointingPossibilitiesReportBuilder(number, ppimg));
                    }
                    else if (ppimg.AreAllInSameColumn())
                    {
                        int col = ppimg.First().Col;
                        for (int row = 0; row < 9; row++)
                        {
                            if (row / 3 != miniRow) strategyManager.ChangeBuffer.AddPossibilityToRemove(number, row, col);
                        }
                        
                        strategyManager.ChangeBuffer.Push(this,
                            new PointingPossibilitiesReportBuilder(number, ppimg));
                    }
                }
            }
        }
    }
}

public class PointingPossibilitiesReportBuilder : IChangeReportBuilder
{
    private readonly int _number;
    private readonly MiniGridPositions _miniPos;

    public PointingPossibilitiesReportBuilder(int number, MiniGridPositions miniPos)
    {
        _number = number;
        _miniPos = miniPos;
    }
    
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var pos in _miniPos)
            {
                lighter.HighlightPossibility(_number, pos.Row, pos.Col, ChangeColoration.CauseOffOne);
            }
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}