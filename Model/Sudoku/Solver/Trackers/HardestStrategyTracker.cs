using System;
using System.Collections.Generic;

namespace Model.Sudoku.Solver.Trackers;

public class HardestStrategyTracker : Tracker
{
    private int _hardestIndex = -1;
    public SudokuStrategy? Hardest { get; private set; }

    public override void OnSolveStart()
    {
        Hardest = null;
        _hardestIndex = -1;
    }

    public override void OnStrategyEnd(SudokuStrategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        if (solutionAdded + possibilitiesRemoved == 0) return;

        if (Hardest is null || Hardest.Difficulty < strategy.Difficulty
                            || Hardest.Difficulty == strategy.Difficulty && index > _hardestIndex)
        {
            Hardest = strategy;
            _hardestIndex = index;
        }
    }
}