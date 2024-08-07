using System;
using System.Collections.Generic;
using System.Linq;
using Model.Core.BackTracking;

namespace Model.Nonograms.BackTrackers;

public class SpareSpaceNonogramBackTracker : BackTracker<Nonogram, IAvailabilityChecker>
{
    private IEnumerable<int[]>?[] _rowCombinations = Array.Empty<IEnumerable<int[]>?>();
    private Nonogram _copy = new();
    
    public SpareSpaceNonogramBackTracker() : base(new Nonogram(), ConstantAvailabilityChecker.Instance) { }
    
    public SpareSpaceNonogramBackTracker(Nonogram puzzle, IAvailabilityChecker data) : base(puzzle, data)
    {
    }

    protected override bool Search(int position)
    {
        if(position < Current.RowCount)
        {
            var spare = Current.ColumnCount - Current.HorizontalLines.NeededSpace(position);
            _rowCombinations[position] ??= SpacePositionsCombination(Current.HorizontalLines.ValueCount(position),
                spare);

            foreach (var spaceRepartition in _rowCombinations[position]!)
            {
                var cursor = 0;
                var pos = 0;
                var ok = true;
                
                foreach (var value in Current.HorizontalLines[position])
                {
                    var posEnd = pos + spaceRepartition[cursor] + (cursor == 0 ? 0 : 1);
                    for (; pos < posEnd; pos++)
                    {
                        if (Current[position, pos])
                        {
                            ok = false;
                            break;
                        }
                    }

                    if (!ok) break;

                    posEnd = pos + value;
                    for (; pos < posEnd; pos++)
                    {
                        if (!_giver.IsAvailable(position, pos))
                        {
                            ok = false; 
                            break;
                        }
                        Current[position, pos] = true;
                    }
                    
                    if (!ok) break;
                    cursor++;
                }

                if (ok && Search(position + 1)) return true;

                pos--;
                for (; pos >= 0; pos--) Current[position, pos] = _copy[position, pos];
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
        if(position < Current.RowCount)
        {
            var spare = Current.ColumnCount - Current.HorizontalLines.NeededSpace(position);
            _rowCombinations[position] ??= SpacePositionsCombination(Current.HorizontalLines.ValueCount(position),
                spare);

            foreach (var spaceRepartition in _rowCombinations[position]!)
            {
                var cursor = 0;
                var pos = 0;
                var ok = true;
                
                foreach (var value in Current.HorizontalLines[position])
                {
                    var posEnd = pos + spaceRepartition[cursor] + (cursor == 0 ? 0 : 1);
                    for (; pos < posEnd; pos++)
                    {
                        if (Current[position, pos])
                        {
                            ok = false;
                            break;
                        }
                    }

                    if (!ok) break;

                    posEnd = pos + value;
                    for (; pos < posEnd; pos++)
                    {
                        Current[position, pos] = true;
                    }

                    cursor++;
                }

                ok = ok && Search(result, position + 1);
                
                pos--;
                for (; pos >= 0; pos--) Current[position, pos] = _copy[position, pos];
                
                if (ok) return true;
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
        _rowCombinations = new IEnumerable<int[]>?[Current.RowCount];
        _copy = Current.Copy();
    }

    private static IEnumerable<int[]> SpacePositionsCombination(int count, int spare)
    {
        if (count == 0) return Enumerable.Empty<int[]>();
        List<int[]> result = new();
        SpacePositionsCombination(result, new List<int>(), count, spare);
        return result;
    }

    private static void SpacePositionsCombination(List<int[]> result, List<int> current, int count, int remaining)
    {
        for (int i = 0; i <= remaining; i++)
        {
            current.Add(i);
            if (current.Count == count) result.Add(current.ToArray());
            else
            {
                if (i == remaining)
                {
                    int ind = current.Count - 1;
                    for (int j = current.Count; j < count; j++) current.Add(0);
                    result.Add(current.ToArray());
                    current.RemoveRange(ind, current.Count - ind);
                    continue;
                }

                SpacePositionsCombination(result, current, count, remaining - i);
            }

            current.RemoveAt(current.Count - 1);
        }
    }
}