using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.CrossSums.Solver;

public class CrossSumSolver : DichotomousStrategySolver<Strategy<ICrossSumSolverData>,
    IDichotomousSolvingState, ICrossSumHighlighter>, ICrossSumSolverData, IDichotomousSolvingState
{
    private CrossSum _cs = new(0, 0);
    private int[] _rowTotals = Array.Empty<int>();
    private int[] _colTotals = Array.Empty<int>();
    private bool[,] _availability = new bool[0, 0];
    private int _completedLines;

    public IReadOnlyCrossSum CrossSum => _cs;
    public int RowCount => _cs.RowCount;
    public int ColumnCount => _cs.ColumnCount;
    
    public override IDichotomousSolvingState StartState { get; protected set; } =
        new DefaultDichotomousSolvingState(0, 0);

    public void SetCrossSum(CrossSum cs)
    {
        _cs = cs;
        _completedLines = 0;

        _availability = new bool[cs.RowCount, cs.ColumnCount];
        (_rowTotals, _colTotals) = cs.GetCurrentTotals();
        
        InitAvailability();
        OnNewSolvable();
    }
    
    protected override IDichotomousSolvingState GetSolvingState()
    {
        return new DefaultDichotomousSolvingState(this);
    }

    public override bool IsResultCorrect()
    {
        return _cs.IsCorrect(_rowTotals, _colTotals);
    }

    public override bool HasSolverFailed()
    {
        return false; //TODO
    }

    protected override void OnChangeMade()
    {
    }

    protected override void ApplyStrategy(Strategy<ICrossSumSolverData> strategy)
    {
        strategy.Apply(this);
    }

    protected override bool IsComplete()
    {
        return _completedLines == _cs.RowCount + _cs.ColumnCount;
    }

    protected override IDichotomousSolvingState ApplyChangesToState(IDichotomousSolvingState state, IEnumerable<DichotomousChange> changes)
    {
        throw new System.NotImplementedException();
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
        _availability[row, col] = false;
        _cs.Choose(row, col);
        
        var val = _cs[row, col];
        _rowTotals[row] += val;
        _colTotals[col] += val;
        
        if(_rowTotals[row] >= _cs.ExpectedForRow(row)) OnHorizontalLineCompletion(row);
        if(_colTotals[col] >= _cs.ExpectedForColumn(col)) OnVerticalLineCompletion(col);

        return true;
    }

    protected override bool RemovePossibility(int row, int col)
    {
        if (!_availability[row, col]) return false;
         
        _currentState = null;
        _availability[row, col] = false;

        return true;
    }

    public int GetTotalForRow(int row) => _rowTotals[row];

    public int GetTotalForColumn(int col) => _colTotals[col];

    public bool this[int row, int col] => _cs.IsChosen(row, col);

    public bool IsAvailable(int row, int col) => _availability[row, col];

    private void InitAvailability()
    {
        for (int row = 0; row < _cs.RowCount; row++)
        {
            var total = _rowTotals[row];
            if(total >= _cs.ExpectedForRow(row)) OnHorizontalLineCompletion(row);
            else if (total > 0)
            {
                for (int col = 0; col < _cs.ColumnCount; col++)
                {
                    if (_cs.IsChosen(row, col)) _availability[row, col] = false;
                }
            }
        }

        for (int col = 0; col < _cs.ColumnCount; col++)
        {
            if (_colTotals[col] >= _cs.ExpectedForColumn(col)) _completedLines++;
        }
    }

    private void OnHorizontalLineCompletion(int row)
    {
        _completedLines++;
        _availability.SetAllValuesInRow(row, false);
    }
    
    private void OnVerticalLineCompletion(int col)
    {
        _completedLines++;
        _availability.SetAllValuesInColumn(col, false);
    }
}