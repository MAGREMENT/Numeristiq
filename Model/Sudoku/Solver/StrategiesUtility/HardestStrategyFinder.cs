using System;

namespace Model.Sudoku.Solver.StrategiesUtility;

public class HardestStrategyFinder
{
    private readonly StrategyInformation[] _info;
    private int _current = -1;

    public StrategyInformation Hardest {
        get
        {
            if (_current == -1) throw new Exception("No strategy found");

            return _info[_current];
        }
    }

    public HardestStrategyFinder(SudokuSolver solver)
    {
        _info = solver.GetStrategyInfo();
        solver.StrategyStopped += (i, a, p) =>
        {
            if (i >= _info.Length || a + p == 0) return;

            if (_current == -1) _current = i;
            else if (_info[_current].Difficulty < _info[i].Difficulty) _current = i;
            else if (_info[_current].Difficulty == _info[i].Difficulty && i > _current) _current = i;
        };
    }

    public void Clear()
    {
        _current = -1;
    }
}