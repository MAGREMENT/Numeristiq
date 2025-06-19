using System;
using Model.Core.BackTracking;

namespace Model.CrossSums;

public class CrossSumBackTracker : BackTracker<CrossSum, IAvailabilityChecker>
{
    private int[] _rowTotals = Array.Empty<int>();
    private int[] _colTotals = Array.Empty<int>();
    
    public CrossSumBackTracker(CrossSum puzzle, IAvailabilityChecker data) : base(puzzle, data)
    {
    }

    public CrossSumBackTracker() : base(new CrossSum(0, 0), 
        ConstantAvailabilityChecker.Instance) {}

    protected override bool Search(int position)
    {
        var total = Current.RowCount * Current.ColumnCount;
        for (; position < total; position++)
        {
            var row = position / Current.ColumnCount;
            var col = position % Current.ColumnCount;
            
            if(!_giver.IsAvailable(row, col) || Current.IsChosen(row, col)) continue;

            var val = Current[row, col];
            var currRow = val + _rowTotals[row];
            var currCol = val + _colTotals[col];

            if (currRow > Current.ExpectedForRow(row) || currCol > Current.ExpectedForColumn(col)) continue;
            
            Current.Choose(row, col);
            _rowTotals[row] = currRow;
            _colTotals[col] = currCol;

            if(Search(position + 1)) return true;

            Current.Choose(row, col, false);
            _rowTotals[row] -= val;
            _colTotals[col] -= val;
        }

        return IsCorrect();
    }

    protected override bool Search(IBackTrackingResult<CrossSum> result, int position)
    {
        var total = Current.RowCount * Current.ColumnCount;
        for (; position < total; position++)
        {
            var row = position / Current.ColumnCount;
            var col = position % Current.ColumnCount;
            
            if(!_giver.IsAvailable(row, col) || Current.IsChosen(row, col)) continue;

            var val = Current[row, col];
            var currRow = val + _rowTotals[row];
            var currCol = val + _colTotals[col];

            if (currRow > Current.ExpectedForRow(row) || currCol > Current.ExpectedForColumn(col)) continue;
            
            Current.Choose(row, col);
            _rowTotals[row] = currRow;
            _colTotals[col] = currCol;

            var found = Search(position + 1);
            Current.Choose(row, col, false);

            if (found) return true;
            
            _rowTotals[row] -= val;
            _colTotals[col] -= val;
        }

        if (!IsCorrect()) return false;
        
        result.AddNewResult(Current.Copy());
        return StopAt >= result.Count;
    }

    private bool IsCorrect()
    {
        for (int r = 0; r < Current.RowCount; r++)
        {
            if (_rowTotals[r] != Current.ExpectedForRow(r)) return false;
        }

        for (int c = 0; c < Current.ColumnCount; c++)
        {
            if (_colTotals[c] != Current.ExpectedForColumn(c)) return false;
        }

        return true;
    }

    protected override void Initialize(bool reset)
    {
        _colTotals = new int[Current.ColumnCount];
        _rowTotals = new int[Current.RowCount];

        for (int i = 0; i < Current.RowCount; i++)
        {
            for (int j = 0; j < Current.ColumnCount; j++)
            {
                if (Current.IsChosen(i, j))
                {
                    var v = Current[i, j];
                    _colTotals[j] += v;
                    _rowTotals[i] += v;
                }
            }
        }
    }
}