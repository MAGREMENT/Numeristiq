using System.Collections.Generic;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class BUGStrategy : IStrategy
{
    public const string OfficialName = "BUG";
    
    public string Name => OfficialName;
    public StrategyDifficulty Difficulty => StrategyDifficulty.Medium;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        var triple = OnlyDoublesAndOneTriple(strategyManager);
        if (triple.Row == -1) return;
        
        foreach (var possibility in strategyManager.PossibilitiesAt(triple.Row, triple.Col))
        {
            if (strategyManager.ColumnPositionsAt(triple.Col, possibility).Count != 3 ||
                strategyManager.RowPositionsAt(triple.Row, possibility).Count != 3 ||
                strategyManager.MiniGridPositionsAt(triple.Row / 3, triple.Col / 3, possibility).Count != 3) 
                continue;
            
            strategyManager.ChangeBuffer.AddSolutionToAdd(possibility, triple.Row, triple.Col);
            break;
        }

        strategyManager.ChangeBuffer.Push(this, new BUGReportBuilder(triple));
        
    }

    private Cell OnlyDoublesAndOneTriple(IStrategyManager strategyManager)
    {
        var triple = new Cell(-1, -1);
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Sudoku[row, col] == 0 && strategyManager.PossibilitiesAt(row, col).Count != 2)
                {
                    if (strategyManager.PossibilitiesAt(row, col).Count != 3 || triple.Row != -1)
                        return new Cell(-1, -1);

                    triple = new Cell(row, col);
                }
            }
        }

        return triple;
    }
}

public class BUGReportBuilder : IChangeReportBuilder
{
    private readonly Cell _triple;

    public BUGReportBuilder(Cell triple)
    {
        _triple = triple;
    }
    
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            lighter.HighlightCell(_triple.Row, _triple.Col, ChangeColoration.CauseOnOne);
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}