using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;

namespace Model.Nonograms.Solver;

public class NonogramSolver : DichotomousStrategySolver<Strategy<INonogramSolverData>, INonogramSolvingState, INonogramHighlighter>,
    INonogramSolverData, IDichotomousSolvingState
{
    private int _completedLines;
    private Nonogram _nonogram = new();
    private bool[,] _availability = new bool[0, 0]; //TODO infinite bitSet
    
    public override INonogramSolvingState StartState { get; protected set; } = new DefaultDichotomousSolvingState(0, 0);
    public IReadOnlyNonogram Nonogram => _nonogram;
    public NonogramPreComputer PreComputer { get; }

    public NonogramSolver()
    {
        PreComputer = new NonogramPreComputer(this);
    }

    public void SetNonogram(Nonogram nonogram)
    {
        _nonogram = nonogram;
        _completedLines = 0;
        _availability = new bool[_nonogram.RowCount, _nonogram.ColumnCount];
        InitAvailability();
        PreComputer.AdaptToNewSize(nonogram.RowCount, nonogram.ColumnCount);
        PreComputer.Reset();
        OnNewSolvable();
    }
    
    protected override INonogramSolvingState GetSolvingState()
    {
        return new DefaultDichotomousSolvingState(_nonogram.RowCount, _nonogram.ColumnCount, this);
    }

    public override bool IsResultCorrect()
    {
        return _nonogram.IsCorrect();
    }

    public override bool HasSolverFailed()
    {
        return false; //TODO
    }

    protected override void OnChangeMade()
    {
        PreComputer.Reset();
    }

    protected override void ApplyStrategy(Strategy<INonogramSolverData> strategy)
    {
        strategy.Apply(this);
    }

    protected override bool IsComplete()
    {
        return _completedLines == _nonogram.RowCount + _nonogram.ColumnCount;
    }

    protected override INonogramSolvingState ApplyChangesToState(INonogramSolvingState state, IEnumerable<DichotomousChange> changes)
    {
        var result = DefaultDichotomousSolvingState.Copy(state);

        foreach (var change in changes)
        {
            result.SetAvailability(change.Row, change.Column, false);
            if (change.Type != ChangeType.SolutionAddition) continue;
            
            result[change.Row, change.Column] = true;
            if (_nonogram.GetRowSolutionCount(change.Row) ==
                _nonogram.HorizontalLineCollection.TotalExpected(change.Row))
            {
                for (int col = 0; col < state.ColumnCount; col++)
                {
                    result.SetAvailability(change.Row, col, false);
                }
            }


            if (_nonogram.GetColumnSolutionCount(change.Column) ==
                _nonogram.VerticalLineCollection.TotalExpected(change.Column))
            {
                for (int row = 0; row < state.RowCount; row++)
                {
                    result.SetAvailability(row, change.Column, false);
                }
            }
        }

        return result;
    }

    public override bool CanRemovePossibility(Cell cell)
    {
        return _availability[cell.Row, cell.Column];
    }

    public override bool CanAddSolution(Cell cell)
    {
        return _availability[cell.Row, cell.Column];
    }

    protected override bool AddSolution(int row, int col)
    {
        if (!_availability[row, col]) return false;

        _currentState = null;
        _nonogram[row, col] = true;
        _availability[row, col] = false;
        
        if (_nonogram.GetRowSolutionCount(row) == _nonogram.HorizontalLineCollection.TotalExpected(row))
            OnHorizontalLineCompletion(row);

        if (_nonogram.GetColumnSolutionCount(col) == _nonogram.VerticalLineCollection.TotalExpected(col))
            OnVerticalLineCompletion(col);

        return true;
    }

    protected override bool RemovePossibility(int row, int col)
    {
        if (!_availability[row, col]) return false;

        _currentState = null;
        _availability[row, col] = false;

        return true;
    }

    public int RowCount => _nonogram.RowCount;
    public int ColumnCount => _nonogram.ColumnCount;

    public bool this[int row, int col] => _nonogram[row, col];

    public bool IsAvailable(int row, int col) => _availability[row, col];

    private void InitAvailability()
    {
        for (int row = 0; row < _nonogram.RowCount; row++)
        {
            var expected = _nonogram.HorizontalLineCollection.TotalExpected(row);
            var current = 0;
            for (int col = 0; col < _nonogram.ColumnCount; col++)
            {
                if (_nonogram[row, col])
                {
                    _availability[row, col] = false;
                    current++;
                }
                else _availability[row, col] = true;
            }

            if (current >= expected) OnHorizontalLineCompletion(row);
        }

        for (int col = 0; col < _nonogram.ColumnCount; col++)
        {
            var expected = _nonogram.VerticalLineCollection.TotalExpected(col);
            var current = 0;
            for (int row = 0; row < _nonogram.RowCount; row++)
            {
                if(_nonogram[row, col]) current++;
            }
            
            if (current >= expected) OnVerticalLineCompletion(col);
        }
    }

    private void OnHorizontalLineCompletion(int row)
    {
        _completedLines++;
        for (int col = 0; col < _nonogram.ColumnCount; col++)
        {
            _availability[row, col] = false;
        }
    }
    
    private void OnVerticalLineCompletion(int col)
    {
        _completedLines++;
        for (int row = 0; row < _nonogram.RowCount; row++)
        {
            _availability[row, col] = false;
        }
    }
}