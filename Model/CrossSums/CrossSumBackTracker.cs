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
            
            if (col == 0 && row > 1 && _rowTotals[row - 1] != Current.GetExpectedForRow(row - 1)) return false;
            if (_colTotals[col] > Current.GetExpectedForColumn(col)) return false;
            
            if(!_giver.IsAvailable(row, col) || Current.IsChosen(row, col)) continue;

            var val = Current[row, col];
            var currRow = val + _rowTotals[row];
            var currCol = val + _colTotals[col];

            if (currRow > Current.GetExpectedForRow(row) || currCol > Current.GetExpectedForColumn(col)) continue;
            
            Current.Choose(row, col);
            _rowTotals[row] = currRow;
            _colTotals[col] = currCol;

            if(Search(position + 1)) return true;

            Current.Choose(row, col, false);
            _rowTotals[row] -= val;
            _colTotals[col] -= val;
        }

        return Current.IsCorrect(_rowTotals, _colTotals);
    }

    protected override bool Search(IBackTrackingResult<CrossSum> result, int position)
    {
        var total = Current.RowCount * Current.ColumnCount;
        for (; position < total; position++)
        {
            var row = position / Current.ColumnCount;
            var col = position % Current.ColumnCount;

            if (col == 0 && row > 1 && _rowTotals[row - 1] != Current.GetExpectedForRow(row - 1)) return false;
            if (_colTotals[col] > Current.GetExpectedForColumn(col)) return false;
            
            if(!_giver.IsAvailable(row, col) || Current.IsChosen(row, col)) continue;

            var val = Current[row, col];
            var currRow = val + _rowTotals[row];
            var currCol = val + _colTotals[col];
            
            if (currRow > Current.GetExpectedForRow(row) || currCol > Current.GetExpectedForColumn(col)) continue;
            
            Current.Choose(row, col);
            _rowTotals[row] = currRow;
            _colTotals[col] = currCol;

            var found = Search(result, position + 1);
            Current.Choose(row, col, false);

            if (found) return true;
            
            _rowTotals[row] -= val;
            _colTotals[col] -= val;
        }

        if (!Current.IsCorrect(_rowTotals, _colTotals)) return false;
        
        result.AddNewResult(Current);
        return StopAt >= result.Count;
    }

    protected override void Initialize(bool reset)
    {
        (_rowTotals, _colTotals) = Current.GetCurrentTotals();
    }
}