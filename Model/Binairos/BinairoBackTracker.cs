using System;
using Model.Core.BackTracking;
using Model.Utility.BitSets;

namespace Model.Binairos;

public class BinairoBackTracker : BackTracker<Binairo, object?>
{
    private InfiniteBitMap _oneUnavailability = new(0, 0);
    private InfiniteBitMap _twoUnavailability = new(0, 0);
    private BinaryUnitCount[] _rowCounts = Array.Empty<BinaryUnitCount>();
    private BinaryUnitCount[] _colCounts = Array.Empty<BinaryUnitCount>();
    
    public BinairoBackTracker(Binairo puzzle) : base(puzzle, null)
    {
    }
    
    public BinairoBackTracker() : base(new Binairo(0, 0), null)
    {
    }

    protected override bool Search(int position)
    {
        var total = Current.RowCount * Current.ColumnCount;
        var halfC = Current.ColumnCount / 2;
        var halfR = Current.RowCount / 2;
        for (; position < total; position++)
        {
            bool uniqueness, added1Col, added2Col, added1Row, added2Row;
            var row = position / Current.ColumnCount;
            var col = position % Current.ColumnCount;
            if (Current[row, col] != 0)
            {
                if (RowUniquenessCheck(row, col) && ColumnUniquenessCheck(row, col)) continue;
                
                return false;
            }

            if (!_oneUnavailability.Contains(row, col) && _rowCounts[row].OnesCount != halfC 
                                                       && _colCounts[col].OnesCount != halfR)
            {
                Current[row, col] = 1;
                uniqueness = RowUniquenessCheck(row, col) && ColumnUniquenessCheck(row, col);
                if (uniqueness)
                {
                    added1Col = false;
                    added2Col = false;
                    added1Row = false;
                    added2Row = false;
                    
                    _rowCounts[row].OnesCount += 1;
                    _colCounts[col].OnesCount += 1;
                    
                    if (col < Current.ColumnCount - 2 && Current[row, col + 1] == 1
                                                      && !_oneUnavailability.Contains(row, col + 2))
                    {
                        _oneUnavailability.Add(row, col + 2);
                        added2Col = true;
                    }
                    if (col < Current.ColumnCount - 1 && col > 0 && Current[row, col - 1] == 1
                                                      && !_oneUnavailability.Contains(row, col + 1))
                    {
                        _oneUnavailability.Add(row, col + 1);
                        added1Col = true;
                    }

                    if (row < Current.RowCount - 2 && Current[row + 1, col] == 1
                                                   && !_oneUnavailability.Contains(row + 2, col))
                    {
                        _oneUnavailability.Add(row + 2, col);
                        added2Row = true;
                    }
                    if (row < Current.RowCount - 1 && row > 0 && Current[row - 1, col] == 1
                                                   && !_oneUnavailability.Contains(row + 1, col))
                    {
                        _oneUnavailability.Add(row + 1, col);
                        added1Row = true;
                    }

                    if (Search(position + 1)) return true;

                    Current[row, col] = 0;
                    _rowCounts[row].OnesCount -= 1;
                    _colCounts[col].OnesCount -= 1;
                    if (added2Col) _oneUnavailability.Remove(row, col + 2);
                    if (added2Row) _oneUnavailability.Remove(row + 2, col);
                    if (added1Col) _oneUnavailability.Remove(row, col + 1);
                    if (added1Row) _oneUnavailability.Remove(row + 1, col);
                }
                else Current[row, col] = 0;
            }

            if (_twoUnavailability.Contains(row, col) || _rowCounts[row].TwosCount == halfC
                                                      || _colCounts[col].TwosCount == halfR) return false;
            
            Current[row, col] = 2;
            uniqueness = RowUniquenessCheck(row, col) && ColumnUniquenessCheck(row, col);
            if (!uniqueness)
            {
                Current[row, col] = 0;
                return false;
            }
            
            added1Col = false;
            added2Col = false;
            added1Row = false;
            added2Row = false;
            _rowCounts[row].TwosCount += 1;
            _colCounts[col].TwosCount += 1;
            
            if (col < Current.ColumnCount - 2 && Current[row, col + 1] == 2 
                                              && !_twoUnavailability.Contains(row, col + 2))
            {
                _twoUnavailability.Add(row, col + 2);
                added2Col = true;
            }
            if (col < Current.ColumnCount - 1 && col > 0 && Current[row, col - 1] == 2 
                                              && !_twoUnavailability.Contains(row, col + 1))
            {
                _twoUnavailability.Add(row, col + 1);
                added1Col = true;
            }

            if (row < Current.RowCount - 2 && Current[row + 1, col] == 2
                                           && !_twoUnavailability.Contains(row + 2, col))
            {
                _twoUnavailability.Add(row + 2, col);
                added2Row = true;
            }
            if (row < Current.RowCount - 1 && row > 0 && Current[row - 1, col] == 2
                                           && !_twoUnavailability.Contains(row + 1, col))
            {
                _twoUnavailability.Add(row + 1, col);
                added1Row = true;
            }

            if (Search(position + 1)) return true;

            Current[row, col] = 0;
            _rowCounts[row].TwosCount -= 1;
            _colCounts[col].TwosCount -= 1;
            if (added1Col) _twoUnavailability.Remove(row, col + 1);
            if (added1Row) _twoUnavailability.Remove(row + 1, col);
            if (added2Col) _twoUnavailability.Remove(row, col + 2);
            if (added2Row) _twoUnavailability.Remove(row + 2, col);

            return false;
        }

        return true;
    }

    protected override bool Search(IBackTrackingResult<Binairo> result, int position) 
    {
        var total = Current.RowCount * Current.ColumnCount;
        var halfC = Current.ColumnCount / 2;
        var halfR = Current.RowCount / 2;
        for (; position < total; position++)
        {
            bool uniqueness, added1Col, added2Col, added1Row, added2Row, search;
            var row = position / Current.ColumnCount;
            var col = position % Current.ColumnCount;
            if (Current[row, col] != 0)
            {
                if (RowUniquenessCheck(row, col) && ColumnUniquenessCheck(row, col)) continue;
                
                return false;
            }

            if (!_oneUnavailability.Contains(row, col) && _rowCounts[row].OnesCount != halfC 
                                                       && _colCounts[col].OnesCount != halfR)
            {
                Current[row, col] = 1;
                uniqueness = RowUniquenessCheck(row, col) && ColumnUniquenessCheck(row, col);
                if (uniqueness)
                {
                    added1Col = false;
                    added2Col = false;
                    added1Row = false;
                    added2Row = false;
                    
                    _rowCounts[row].OnesCount += 1;
                    _colCounts[col].OnesCount += 1;
                    
                    if (col < Current.ColumnCount - 2 && Current[row, col + 1] == 1
                                                      && !_oneUnavailability.Contains(row, col + 2))
                    {
                        _oneUnavailability.Add(row, col + 2);
                        added2Col = true;
                    }
                    if (col < Current.ColumnCount - 1 && col > 0 && Current[row, col - 1] == 1
                                                      && !_oneUnavailability.Contains(row, col + 1))
                    {
                        _oneUnavailability.Add(row, col + 1);
                        added1Col = true;
                    }

                    if (row < Current.RowCount - 2 && Current[row + 1, col] == 1
                                                   && !_oneUnavailability.Contains(row + 2, col))
                    {
                        _oneUnavailability.Add(row + 2, col);
                        added2Row = true;
                    }
                    if (row < Current.RowCount - 1 && row > 0 && Current[row - 1, col] == 1
                                                   && !_oneUnavailability.Contains(row + 1, col))
                    {
                        _oneUnavailability.Add(row + 1, col);
                        added1Row = true;
                    }

                    if (Search(result, position + 1))
                    {
                        Current[row, col] = 0;
                        return true;
                    }

                    Current[row, col] = 0;
                    _rowCounts[row].OnesCount -= 1;
                    _colCounts[col].OnesCount -= 1;
                    if (added2Col) _oneUnavailability.Remove(row, col + 2);
                    if (added2Row) _oneUnavailability.Remove(row + 2, col);
                    if (added1Col) _oneUnavailability.Remove(row, col + 1);
                    if (added1Row) _oneUnavailability.Remove(row + 1, col);
                }
                else Current[row, col] = 0;
            }

            if (_twoUnavailability.Contains(row, col) || _rowCounts[row].TwosCount == halfC
                                                      || _colCounts[col].TwosCount == halfR) return false;
            
            Current[row, col] = 2;
            uniqueness = RowUniquenessCheck(row, col) && ColumnUniquenessCheck(row, col);
            if (!uniqueness)
            {
                Current[row, col] = 0;
                return false;
            }
            
            added1Col = false;
            added2Col = false;
            added1Row = false;
            added2Row = false;
            _rowCounts[row].TwosCount += 1;
            _colCounts[col].TwosCount += 1;
            
            if (col < Current.ColumnCount - 2 && Current[row, col + 1] == 2 
                                              && !_twoUnavailability.Contains(row, col + 2))
            {
                _twoUnavailability.Add(row, col + 2);
                added2Col = true;
            }
            if (col < Current.ColumnCount - 1 && col > 0 && Current[row, col - 1] == 2 
                                              && !_twoUnavailability.Contains(row, col + 1))
            {
                _twoUnavailability.Add(row, col + 1);
                added1Col = true;
            }

            if (row < Current.RowCount - 2 && Current[row + 1, col] == 2
                                           && !_twoUnavailability.Contains(row + 2, col))
            {
                _twoUnavailability.Add(row + 2, col);
                added2Row = true;
            }
            if (row < Current.RowCount - 1 && row > 0 && Current[row - 1, col] == 2
                                           && !_twoUnavailability.Contains(row + 1, col))
            {
                _twoUnavailability.Add(row + 1, col);
                added1Row = true;
            }

            if (Search(result, position + 1))
            {
                Current[row, col] = 0;
                return true;
            }

            Current[row, col] = 0;
            _rowCounts[row].TwosCount -= 1;
            _colCounts[col].TwosCount -= 1;
            if (added1Col) _twoUnavailability.Remove(row, col + 1);
            if (added1Row) _twoUnavailability.Remove(row + 1, col);
            if (added2Col) _twoUnavailability.Remove(row, col + 2);
            if (added2Row) _twoUnavailability.Remove(row + 2, col);

            return false;
        }

        result.AddNewResult(Current);
        return result.Count >= StopAt;
    }

    protected override void Initialize(bool reset)
    {
        _oneUnavailability = new InfiniteBitMap(Current.RowCount, Current.ColumnCount);
        _twoUnavailability = new InfiniteBitMap(Current.RowCount, Current.ColumnCount);
        _rowCounts = new BinaryUnitCount[Current.RowCount];
        _colCounts = new BinaryUnitCount[Current.ColumnCount];

        for (int row = 0; row < Current.RowCount; row++)
        {
            for (int col = 0; col < Current.ColumnCount; col++)
            {
                var n = Current[row, col];
                InfiniteBitMap toAddTo;
                switch (n)
                {
                    case 0:
                        continue;
                    case 1:
                        _rowCounts[row].OnesCount++;
                        _colCounts[col].OnesCount++;
                        toAddTo = _oneUnavailability;
                        break;
                    default:
                        _rowCounts[row].TwosCount++;
                        _colCounts[col].TwosCount++;
                        toAddTo = _twoUnavailability;
                        break;
                }
                
                if (col < Current.ColumnCount - 2 && Current[row, col + 2] == n) toAddTo.Add(row, col + 1);
                if (col < Current.ColumnCount - 1 && Current[row, col + 1] == n)
                {
                    if (col < Current.ColumnCount - 2) toAddTo.Add(row, col + 2);
                    if (col > 0) toAddTo.Add(row, col - 1);
                }
                
                if (row < Current.RowCount - 2 && Current[row + 2, col] == n) toAddTo.Add(row + 1, col);
                if (row < Current.RowCount - 1 && Current[row + 1, col] == n)
                {
                    if (row < Current.RowCount - 2) toAddTo.Add(row + 2, col);
                    if (row > 0) toAddTo.Add(row - 1, col);
                }
            }
        }
    }

    private bool RowUniquenessCheck(int row, int col)
    {
        if (col != Current.ColumnCount - 1) return true;

        var set = Current.RowSetAt(row);
        for (int r = 0; r < row; r++)
        {
            if (Current.RowSetAt(r) == set) return false;
        }

        return true;
    }

    private bool ColumnUniquenessCheck(int row, int col)
    {
        if (row != Current.RowCount - 1) return true;

        var set = Current.ColumnSetAt(col);
        for (int c = 0; c < col; c++)
        {
            if (Current.ColumnSetAt(c) == set) return false;
        }

        return true;
    }
}

public struct BinaryUnitCount
{
    public int OnesCount { get; set; }
    public int TwosCount { get; set; }
}