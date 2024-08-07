using System;
using Model.Core.BackTracking;
using Model.Nonograms.Solver;

namespace Model.Nonograms.BackTrackers;

public class ValueSpacesNonogramBackTracker : BackTracker<Nonogram, IAvailabilityChecker>
{
    private ValueSpaceCollection?[] _collections = Array.Empty<ValueSpaceCollection?>();
    private Nonogram _copy = new();
    
    public ValueSpacesNonogramBackTracker() : base(new Nonogram(), ConstantAvailabilityChecker.Instance){}
    
    public ValueSpacesNonogramBackTracker(Nonogram puzzle, IAvailabilityChecker data) : base(puzzle, data)
    {
    }

    protected override bool Search(int position)
    {
        for (; position < Current.RowCount; position++)
        {
            _collections[position] ??= NonogramUtility.HorizontalValueSpaces(Current, _giver,
                NonogramUtility.HorizontalRemainingValuesSpace(Current, _giver, position), position);
            var collection = _collections[position]!;
            if(collection.Count == 0) continue;
            
            var indexes = new int[collection.Count];
            var run = true;
            while (run)
            {
                bool ok = true;
                
                for (int i = 0; i < collection.Count; i++)
                {
                    var start = collection[i].Start + indexes[i];
                    for (int n = start; n < start + collection[i].Value; n++)
                    {
                        if (!_giver.IsAvailable(position, n))
                        {
                            ok = false;
                            break;
                        }

                        Current[position, n] = true;
                    }

                    if (!ok) break;
                }
                
                if (ok && Search(position + 1)) return true;
                for (int n = collection[0].Start; n <= collection[^1].End; n++) Current[position, n] = _copy[position, n];

                for (int n = 0; n < indexes.Length; n++)
                {
                    if (indexes[n] == collection[n].GetLength() - collection[n].Value - 1)
                    {
                        if (n == indexes.Length - 1) run = false;
                        indexes[n] = 0;
                    }
                    else
                    {
                        indexes[n]++;
                        break;
                    }
                }
            }

            return false;
        }
        
        for (int col = 0; col < Current.ColumnCount; col++)
        {
            if (Current.IsColumnCorrect(col)) continue;

            return false;
        }

        return true;
    }

    protected override bool Search(IBackTrackingResult<Nonogram> result, int position)
    {
        for (; position < Current.RowCount; position++)
        {
            _collections[position] ??= NonogramUtility.HorizontalValueSpaces(Current, _giver,
                NonogramUtility.HorizontalRemainingValuesSpace(Current, _giver, position), position);
            var collection = _collections[position]!;
            if(collection.Count == 0) continue;
            
            var indexes = new int[collection.Count];
            var run = true;
            while (run)
            {
                bool ok = true;
                
                for (int i = 0; i < collection.Count; i++)
                {
                    var start = collection[i].Start + indexes[i];
                    for (int n = start; n < start + collection[i].Value; n++)
                    {
                        if (!_giver.IsAvailable(position, n))
                        {
                            ok = false;
                            break;
                        }

                        Current[position, n] = true;
                    }

                    if (!ok) break;
                }

                ok = ok && Search(result, position + 1);
                
                for (int n = collection[0].Start; n <= collection[^1].End; n++) Current[position, n] = _copy[position, n];
                if (ok) return true;
                
                for (int n = 0; n < indexes.Length; n++)
                {
                    if (indexes[n] == collection[n].GetLength() - collection[n].Value - 1)
                    {
                        if (n == indexes.Length - 1) run = false;
                        indexes[n] = 0;
                    }
                    else
                    {
                        indexes[n]++;
                        break;
                    }
                }
            }

            return false;
        }
        
        for (int col = 0; col < Current.ColumnCount; col++)
        {
            if (Current.IsColumnCorrect(col)) continue;

            return false;
        }

        result.AddNewResult(Current.Copy());
        return result.Count >= StopAt;
    }

    protected override void Initialize(bool reset)
    {
        _collections = new ValueSpaceCollection?[Current.RowCount];
        _copy = Current.Copy();
    }
}