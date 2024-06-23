using System.Collections.Generic;
using Model.Core;
using Model.Kakuros;
using Model.Utility;

namespace Model.Nonograms;

public class NonogramSolver : DichotomousStrategySolver<Strategy<INonogramSolverData>, IUpdatableDichotomousSolvingState, object>,
    INonogramSolverData
{
    private int _completedLines;
    private Nonogram _nonogram = new();
    private bool[,] _availability = new bool[0, 0]; //TODO infinite bitSet
    
    public override IUpdatableDichotomousSolvingState StartState { get; protected set; }
    public IReadOnlyNonogram Nonogram => _nonogram;

    public NonogramSolver()
    {
        StartState = new NonogramSolvingState();
    }

    public void SetNonogram(Nonogram nonogram)
    {
        _nonogram = nonogram;
        _completedLines = 0;
        _availability = new bool[_nonogram.RowCount, _nonogram.ColumnCount];
        InitAvailability();
        OnNewSolvable();
    }
    
    protected override IUpdatableDichotomousSolvingState GetSolvingState()
    {
        return new NonogramSolvingState();
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
        
    }

    protected override void ApplyStrategy(Strategy<INonogramSolverData> strategy)
    {
        strategy.Apply(this);
    }

    protected override bool IsComplete()
    {
        return _completedLines == _nonogram.RowCount + _nonogram.ColumnCount;
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

        var current = 0;
        for (int r = 0; r < _nonogram.RowCount; r++)
        {
            if (_nonogram[r, col]) current++;
        }

        if (current == _nonogram.VerticalLineCollection.TotalExpected(col)) OnVerticalLineCompletion(col);
        
        current = 0;
        for (int c = 0; c < _nonogram.RowCount; c++)
        {
            if (_nonogram[row, c]) current++;
        }

        if (current == _nonogram.HorizontalLineCollection.TotalExpected(row)) OnHorizontalLineCompletion(row);

        return true;
    }

    protected override bool RemovePossibility(int row, int col)
    {
        if (_availability[row, col]) return false;

        _currentState = null;
        _availability[row, col] = false;

        return true;
    }

    public bool IsAvailable(int row, int col) => _availability[row, col];

    public IEnumerable<LineSpace> EnumerateSpaces(Orientation orientation, int index)
    {
        return orientation == Orientation.Horizontal
            ? _nonogram.EnumerateHorizontalSpaces(index)
            : _nonogram.EnumerateVerticalSpaces(index);
    }

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