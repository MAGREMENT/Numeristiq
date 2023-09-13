using System.Collections.Generic;
using Model.Changes;
using Model.Solver;

namespace Model.Strategies;

public class BUGStrategy : IStrategy
{
    public string Name { get; } = "BUG";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Medium;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        int[]? triple = OnlyDoublesAndOneTriple(strategyManager);
        if (triple is not null)
        {
            foreach (var possibility in strategyManager.Possibilities[triple[0], triple[1]])
            {
                if (strategyManager.ColumnPositionsAt(triple[1], possibility).Count == 3 &&
                    strategyManager.RowPositionsAt(triple[0], possibility).Count == 3 &&
                    strategyManager.MiniGridPositionsAt(triple[0] / 3, triple[1] / 3, possibility).Count == 3)
                {
                    strategyManager.ChangeBuffer.AddDefinitiveToAdd(possibility, triple[0], triple[1]);
                    break;
                }
            }

            strategyManager.ChangeBuffer.Push(this, new BUGReportBuilder(triple));
        }
    }

    private int[]? OnlyDoublesAndOneTriple(IStrategyManager strategyManager)
    {
        int[]? triple = null;
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Sudoku[row, col] == 0 && strategyManager.Possibilities[row, col].Count != 2)
                {
                    if (strategyManager.Possibilities[row, col].Count != 3 || triple is not null) return null;
                    triple = new[] { row, col };
                }
            }
        }

        return triple;
    }
}

public class BUGReportBuilder : IChangeReportBuilder
{
    private readonly int[] _triple;

    public BUGReportBuilder(int[] triple)
    {
        _triple = triple;
    }
    
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return ChangeReport.Default(changes);
    }
}