using System.Collections.Generic;

namespace Model.Sudoku.Solver.Trackers;

public class UsedStrategiesTracker : Tracker
{
    private readonly HashSet<string> _used = new();

    public override void OnSolveStart()
    {
        _used.Clear();
    }

    public override void OnStrategyEnd(SudokuStrategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        if (solutionAdded + possibilitiesRemoved == 0) return;

        _used.Add(strategy.Name);
    }

    public bool WasUsed(string strategyName) => _used.Contains(strategyName);
}