using System.Collections.Generic;
using Model.Changes;
using Model.Positions;
using Model.Solver;

namespace Model.Strategies;

public class HiddenDoublesStrategy : IStrategy
{
    public string Name => "Hidden doubles";
    public StrategyLevel Difficulty => StrategyLevel.Easy;
    public StatisticsTracker Tracker { get; } = new();
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        Dictionary<IReadOnlyLinePositions, int> lines = new();
        Dictionary<IReadOnlyMiniGridPositions, int> minis = new();
        for (int row = 0; row < 9; row++)
        {
            for (int number = 1; number <= 9; number++)
            {
                var pos = strategyManager.RowPositionsAt(row, number);
                if (pos.Count != 2) continue;

                if (lines.TryGetValue(pos, out var n)) ProcessRow(strategyManager, pos, row, number, n);
                else lines.Add(pos, number);
            }

            lines.Clear();
        }

        for (int col = 0; col < 9; col++)
        {
            for (int number = 1; number <= 9; number++)
            {
                var pos = strategyManager.ColumnPositionsAt(col, number);
                if (pos.Count != 2) continue;

                if (lines.TryGetValue(pos, out var n)) ProcessColumn(strategyManager, pos, col, number, n);
                else lines.Add(pos, number);
            }

            lines.Clear();
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                for (int number = 1; number <= 9; number++)
                {
                    var pos = strategyManager.MiniGridPositionsAt(miniRow, miniCol, number);
                    if (pos.Count != 2) continue;

                    if (minis.TryGetValue(pos, out var n)) ProcessMiniGrid(strategyManager, pos, number, n);
                    else minis.Add(pos, number);
                }

                minis.Clear();
            }
        }
    }
    
    private void ProcessRow(IStrategyManager strategyManager, IReadOnlyLinePositions positions, int row, int n1,
        int n2)
    {
        foreach (var col in positions)
        {
            foreach (var possibility in strategyManager.PossibilitiesAt(row, col))
            {
                if(possibility == n1 || possibility == n2) continue;

                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
            }
        }

        strategyManager.ChangeBuffer.Push(this, new LineHiddenDoublesReportBuilder());
    }

    private void ProcessColumn(IStrategyManager strategyManager, IReadOnlyLinePositions positions, int col,
        int n1, int n2)
    {
        foreach(var row in positions)
        {
            foreach (var possibility in strategyManager.PossibilitiesAt(row, col))
            {
                if(possibility == n1 || possibility == n2) continue;

                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
            }
        }

        strategyManager.ChangeBuffer.Push(this, new LineHiddenDoublesReportBuilder());
    }

    private void ProcessMiniGrid(IStrategyManager strategyManager, IReadOnlyMiniGridPositions positions, int n1, int n2)
    {
        foreach (var cell in positions)
        {
            foreach (var possibility in strategyManager.PossibilitiesAt(cell.Row, cell.Col))
            {
                if(possibility == n1 || possibility == n2) continue;

                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, cell.Row, cell.Col);
            }
        }
        strategyManager.ChangeBuffer.Push(this, new MiniGridHiddenDoublesReportBuilder());
    }
}

public class LineHiddenDoublesReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return ChangeReport.Default(changes);
    }
}

public class MiniGridHiddenDoublesReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return ChangeReport.Default(changes);
    }
}