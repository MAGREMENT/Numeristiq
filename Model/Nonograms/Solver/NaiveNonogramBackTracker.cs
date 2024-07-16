using System.Collections.Generic;
using Model.Core.BackTracking;

namespace Model.Nonograms.Solver;

public class NaiveNonogramBackTracker : BackTracker<Nonogram, IAvailabilityChecker>
{
    public NaiveNonogramBackTracker() : base(new Nonogram(), ConstantAvailabilityChecker.Instance) { }
    
    public NaiveNonogramBackTracker(Nonogram puzzle, IAvailabilityChecker data) : base(puzzle, data)
    {
    }

    protected override bool Search(int position)
    {
        for (; position < Current.RowCount; position++)
        {
            var count = Current.GetRowSolutionCount(position);
            var expected = Current.HorizontalLineCollection.TotalExpected(position);
            
            if (count > expected) return false;
            if (count == expected && Current.IsHorizontalLineCorrect(position)) continue;

            foreach (var combination in RowCombinations(position, expected - count))
            {
                foreach (var c in combination)
                {
                    Current[position, c] = true;
                }

                if (Current.IsHorizontalLineCorrect(position) && Search(position + 1)) return true;
                
                foreach (var c in combination)
                {
                    Current[position, c] = false;
                }
            }
            
            return false;
        }

        for (; position < Current.RowCount + Current.ColumnCount; position++)
        {
            var col = position - Current.RowCount;
            var count = Current.GetColumnSolutionCount(col);
            var expected = Current.VerticalLineCollection.TotalExpected(col);
            
            if (count > expected) return false;
            if (count == expected && Current.IsVerticalLineCorrect(col)) continue;

            foreach (var combination in ColumnCombinations(col, expected - count))
            {
                foreach (var r in combination)
                {
                    Current[r, col] = true;
                }

                if(Current.IsVerticalLineCorrect(col) && Search(position + 1)) return true;
                
                foreach (var r in combination)
                {
                    Current[r, col] = false;
                }
            }

            return false;
        }

        return true;
    }

    protected override bool Search(IBackTrackingResult<Nonogram> result, int position)
    {
        for (; position < Current.RowCount; position++)
        {
            var count = Current.GetRowSolutionCount(position);
            var expected = Current.HorizontalLineCollection.TotalExpected(position);
            
            if (count > expected) return false;
            if (count == expected && Current.IsHorizontalLineCorrect(position)) continue;

            foreach (var combination in RowCombinations(position, expected - count))
            {
                foreach (var c in combination)
                {
                    Current[position, c] = true;
                }

                var search = Current.IsHorizontalLineCorrect(position) && Search(result, position + 1);
                
                foreach (var c in combination)
                {
                    Current[position, c] = false;
                }

                if (search) return true;
            }
            
            return false;
        }

        for (; position < Current.RowCount + Current.ColumnCount; position++)
        {
            var col = position - Current.RowCount;
            var count = Current.GetColumnSolutionCount(col);
            var expected = Current.VerticalLineCollection.TotalExpected(col);
            
            if (count > expected) return false;
            if (count == expected && Current.IsVerticalLineCorrect(col)) continue;

            foreach (var combination in ColumnCombinations(col, expected - count))
            {
                foreach (var r in combination)
                {
                    Current[r, col] = true;
                }

                var search = Current.IsVerticalLineCorrect(col) && Search(result, position + 1);
                
                foreach (var r in combination)
                {
                    Current[r, col] = false;
                }

                if (search) return true;
            }

            return false;
        }
        
        result.AddNewResult(Current.Copy());
        return result.Count >= StopAt;
    }

    protected override void Initialize(bool reset)
    {
        
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
    
    private IEnumerable<IEnumerable<int>> ColumnCombinations(int col, int toAdd)
    {
        List<IEnumerable<int>> result = new();
        SearchColumnCombinations(result, new List<int>(), 0, col, toAdd);
        return result;
    }

    private void SearchColumnCombinations(List<IEnumerable<int>> result, List<int> current, int pos, int col, int toAdd)
    {
        for (; pos < Current.RowCount; pos++)
        {
            if (Current[pos, col] || !_giver.IsAvailable(pos, col)) continue;

            current.Add(pos);
            
            if (current.Count == toAdd) result.Add(current.ToArray());
            else SearchColumnCombinations(result, current, pos + 1, col, toAdd);
            
            current.RemoveAt(current.Count - 1);
        }
    }
}