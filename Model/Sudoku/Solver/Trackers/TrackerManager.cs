using System.Collections.Generic;

namespace Model.Sudoku.Solver.Trackers;

public class TrackerManager
{
    private List<Tracker>? _trackers;
    private readonly SudokuSolver _solver;

    public TrackerManager(SudokuSolver solver)
    {
        _solver = solver;
    }

    public void AddTracker(Tracker tracker)
    {
        tracker.Prepare(_solver);
        _trackers ??= new List<Tracker>();
        _trackers.Add(tracker);
    }

    public void RemoveTracker(Tracker tracker)
    {
        if (_trackers is null) return;
        _trackers.Remove(tracker);
        if (_trackers.Count == 0) _trackers = null;
    }

    public void OnSolveStart()
    {
        if (_trackers is null) return;

        foreach (var tracker in _trackers)
        {
            tracker.OnSolveStart();
        }
    }

    public void OnStrategyStart(SudokuStrategy strategy, int index)
    {
        if (_trackers is null) return;

        foreach (var tracker in _trackers)
        {
            tracker.OnStrategyStart(strategy, index);
        }
    }

    public void OnStrategyEnd(SudokuStrategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        if (_trackers is null) return;

        foreach (var tracker in _trackers)
        {
            tracker.OnStrategyEnd(strategy, index, solutionAdded, possibilitiesRemoved);
        }
    }

    public void OnSolveDone(ISolveResult result)
    {
        if (_trackers is null) return;

        foreach (var tracker in _trackers)
        {
            tracker.OnSolveDone(result);
        }
    }
}