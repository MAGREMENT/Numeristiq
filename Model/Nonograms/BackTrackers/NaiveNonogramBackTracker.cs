using System;
using System.Collections.Generic;
using Model.Core.BackTracking;

namespace Model.Nonograms.BackTrackers;

public class NaiveNonogramBackTracker : BackTracker<Nonogram, IAvailabilityChecker>
{
    private IEnumerable<IEnumerable<int>>?[] _rowCombinations = Array.Empty<IEnumerable<IEnumerable<int>>?>();
    
    public NaiveNonogramBackTracker() : base(new Nonogram(), ConstantAvailabilityChecker.Instance) { }
    
    public NaiveNonogramBackTracker(Nonogram puzzle, IAvailabilityChecker data) : base(puzzle, data)
    {
    }

    protected override bool Search(int position)
    {
        for (; position < Current.RowCount; position++)
        {
            if (Current.IsRowCorrect(position)) continue;
            
            var count = Current.GetRowSolutionCount(position);
            var expected = Current.HorizontalLines.TotalExpected(position);
            
            _rowCombinations[position] ??= RowCombinations(position, expected - count);
            foreach (var combination in _rowCombinations[position]!)
            {
                foreach (var c in combination)
                {
                    Current[position, c] = true;
                }

                if (Current.IsRowCorrect(position) && Search(position + 1)) return true;
                
                foreach (var c in combination)
                {
                    Current[position, c] = false;
                }
            }
            
            return false;
        }

        for (int col = 0; col < Current.ColumnCount; col++)
        {
            if (!Current.IsColumnCorrect(col)) return false;
        }

        return true;
    }

    protected override bool Search(IBackTrackingResult<Nonogram> result, int position)
    {
        for (; position < Current.RowCount; position++)
        {
            if (Current.IsRowCorrect(position)) continue;
            
            var count = Current.GetRowSolutionCount(position);
            var expected = Current.HorizontalLines.TotalExpected(position);
            
            _rowCombinations[position] ??= RowCombinations(position, expected - count);
            foreach (var combination in _rowCombinations[position]!)
            {
                foreach (var c in combination)
                {
                    Current[position, c] = true;
                }

                var search = Current.IsRowCorrect(position) && Search(result, position + 1);
                
                foreach (var c in combination)
                {
                    Current[position, c] = false;
                }

                if (search) return true;
            }
            
            return false;
        }

        for (int col = 0; col < Current.ColumnCount; col++)
        {
            if (!Current.IsColumnCorrect(col)) return false;
        }
        
        result.AddNewResult(Current.Copy());
        return result.Count >= StopAt;
    }

    protected override void Initialize(bool reset)
    {
        _rowCombinations = new IEnumerable<IEnumerable<int>>?[Current.RowCount];
    }

    private IEnumerable<IEnumerable<int>> RowCombinations(int row, int toAdd)
    {
        List<IEnumerable<int>> result = new();
        SearchRowCombinations(result, new List<int>(), 0, row, toAdd);
        return result;
    }

    private void SearchRowCombinations(List<IEnumerable<int>> result, List<int> current, int pos, int row, int toAdd)
    {
        for (; pos < Current.ColumnCount; pos++)
        {
            if (Current[row, pos] || !_giver.IsAvailable(row, pos)) continue;

            current.Add(pos);
            
            if (current.Count == toAdd) result.Add(current.ToArray());
            else SearchRowCombinations(result, current, pos + 1, row, toAdd);
            
            current.RemoveAt(current.Count - 1);
        }
    }
}