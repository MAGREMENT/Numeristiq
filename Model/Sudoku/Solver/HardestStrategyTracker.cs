using System;
using System.Collections.Generic;

namespace Model.Sudoku.Solver;

public class HardestStrategyTracker
{
    private readonly IReadOnlyList<SudokuStrategy> _s;
    private int _current = -1;

    public SudokuStrategy Hardest {
        get
        {
            if (_current == -1) throw new Exception("No strategy found");

            return _s[_current];
        }
    }

    public HardestStrategyTracker(SudokuSolver solver)
    {
        _s = solver.StrategyManager.Strategies;
        solver.StrategyStopped += (i, a, p) =>
        {
            if (i >= _s.Count || a + p == 0) return;

            if (_current == -1) _current = i;
            else if (_s[_current].Difficulty < _s[i].Difficulty) _current = i;
            else if (_s[_current].Difficulty == _s[i].Difficulty && i > _current) _current = i;
        };
    }

    public void Clear()
    {
        _current = -1;
    }
}