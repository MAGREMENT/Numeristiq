using System.Collections.Generic;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Possibilities;

namespace Model.Solver.Strategies;

public class NakedDoublesStrategy : IStrategy
{
    public string Name => "Naked doubles";
    public StrategyLevel Difficulty => StrategyLevel.Easy;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        Dictionary<IReadOnlyPossibilities, int> dict = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var pos = strategyManager.PossibilitiesAt(row, col);
                if (pos.Count != 2) continue;

                if (dict.TryGetValue(pos, out var otherCol)) ProcessRow(strategyManager, pos, row, col, otherCol);
                else dict.Add(pos, col);
            }

            dict.Clear();
        }

        for (int col = 0; col < 9; col++)
        {
            for (int row = 0; row < 9; row++)
            {
                var pos = strategyManager.PossibilitiesAt(row, col);
                if (pos.Count != 2) continue;

                if (dict.TryGetValue(pos, out var otherRow)) ProcessColumn(strategyManager, pos, col, row, otherRow);
                else dict.Add(pos, row);
            }

            dict.Clear();
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                int startRow = miniRow * 3;
                int startCol = miniCol * 3;

                for (int gridRow = 0; gridRow < 3; gridRow++)
                {
                    for (int gridCol = 0; gridCol < 3; gridCol++)
                    {
                        int row = startRow + gridRow;
                        int col = startCol + gridCol;

                        var pos = strategyManager.PossibilitiesAt(row, col);
                        if (pos.Count != 2) continue;

                        var gridNumber = gridRow * 3 + gridCol;
                        if (dict.TryGetValue(pos, out var otherGridNumber))
                            ProcessMiniGrid(strategyManager, pos, miniRow, miniCol, gridNumber, otherGridNumber);
                        else dict.Add(pos, gridNumber);
                    }
                }
                
                dict.Clear();
            }
        }
    }

    private void ProcessRow(IStrategyManager strategyManager, IReadOnlyPossibilities possibilities, int row, int col1,
        int col2)
    {
        for (int col = 0; col < 9; col++)
        {
            if (col == col1 || col == col2) continue;

            foreach (var possibility in possibilities)
            {
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
            }
        }

        strategyManager.ChangeBuffer.Push(this, new LineNakedDoublesReportBuilder());
    }

    private void ProcessColumn(IStrategyManager strategyManager, IReadOnlyPossibilities possibilities, int col,
        int row1, int row2)
    {
        for (int row = 0; row < 9; row++)
        {
            if (row == row1 || row == row2) continue;

            foreach (var possibility in possibilities)
            {
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
            }
        }

        strategyManager.ChangeBuffer.Push(this, new LineNakedDoublesReportBuilder());
    }

    private void ProcessMiniGrid(IStrategyManager strategyManager, IReadOnlyPossibilities possibilities,
        int miniRow, int miniCol, int gridNumber1, int gridNumber2)
    {
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                int gridNumber = gridRow * 3 + gridCol;
                if (gridNumber == gridNumber1 || gridNumber == gridNumber2) continue;

                int row = miniRow * 3 + gridRow;
                int col = miniCol * 3 + gridCol;
                foreach (var possibility in possibilities)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                }
            }
        }

        strategyManager.ChangeBuffer.Push(this, new MiniGridNakedDoublesReportBuilder());
    }

}

public class LineNakedDoublesReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return ChangeReport.Default(changes);
    }
}

public class MiniGridNakedDoublesReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return ChangeReport.Default(changes);
    }
}