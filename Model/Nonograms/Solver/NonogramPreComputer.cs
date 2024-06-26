using System;
using Model.Utility.BitSets;

namespace Model.Nonograms.Solver;

public class NonogramPreComputer
{
    private INonogramSolverData _data;
    
    private LineSpace[] _horizontalMainSpace = Array.Empty<LineSpace>();
    private LineSpace[] _verticalMainSpace = Array.Empty<LineSpace>();
    private readonly InfiniteBitSet _hmsHits = new();
    private readonly InfiniteBitSet _vmsHits = new();

    public NonogramPreComputer(INonogramSolverData data)
    {
        _data = data;
    }

    public void AdaptToNewSize(int rowCount, int colCount)
    {
        _horizontalMainSpace = new LineSpace[rowCount];
        _verticalMainSpace = new LineSpace[colCount];
    }

    public LineSpace HorizontalMainSpace(int row)
    {
        if (!_hmsHits.IsSet(row)) _horizontalMainSpace[row] = DoHorizontalMainSpace(row);
        return _horizontalMainSpace[row];
    }

    public LineSpace VerticalMainSpace(int col)
    {
        if (!_vmsHits.IsSet(col)) _verticalMainSpace[col] = DoVerticalMainSpace(col);
        return _verticalMainSpace[col];
    }

    public void Reset()
    {
        _hmsHits.Clear();
        _vmsHits.Clear();
    }
    
    private LineSpace DoHorizontalMainSpace(int row)
    {
        var list = _data.Nonogram.HorizontalLineCollection.AsList(row);
        int sCursor = 0;
        int eCursor = list.Count - 1;

        var buffer = 0;
        var presenceExpected = 0;
        var mustBeUnavailable = false;
        var start = 0;
        while (start < _data.Nonogram.ColumnCount)
        {
            if (presenceExpected > 0)
            {
                if (!_data.Nonogram[row, start])
                {
                    start = buffer;
                    break;
                }

                presenceExpected--;
                if (presenceExpected == 0)
                {
                    mustBeUnavailable = true;
                    sCursor++;
                }
            }
            else if (mustBeUnavailable)
            {
                if (_data.IsAvailable(row, start)) return LineSpace.Invalid;

                mustBeUnavailable = false;
            }
            else
            {
                if (_data.IsAvailable(row, start)) break;

                if (_data.Nonogram[row, start])
                {
                    presenceExpected = list[sCursor] - 1;
                    if (presenceExpected == 0)
                    {
                        mustBeUnavailable = true;
                        sCursor++;
                    } else buffer = start;
                }
            }

            start++;
        }
        
        presenceExpected = 0;
        mustBeUnavailable = false;
        var end = _data.Nonogram.ColumnCount - 1;
        while (end >= 0)
        {
            if (presenceExpected > 0)
            {
                if (!_data.Nonogram[row, end])
                {
                    end = buffer;
                    break;
                }

                presenceExpected--;
                if (presenceExpected == 0)
                {
                    mustBeUnavailable = true;
                    eCursor--;
                    if (eCursor < sCursor) return LineSpace.Invalid;
                }
            }
            else if (mustBeUnavailable)
            {
                if (_data.IsAvailable(row, end)) return LineSpace.Invalid;

                mustBeUnavailable = false;
            }
            else
            {
                if (_data.IsAvailable(row, end)) break;

                if (_data.Nonogram[row, end])
                {
                    presenceExpected = list[eCursor] - 1;
                    if (presenceExpected == 0)
                    {
                        mustBeUnavailable = true;
                        eCursor--;
                        if (eCursor < sCursor) return LineSpace.Invalid;
                    } else buffer = start;
                }
            }

            end--;
            if (end < start) return LineSpace.Invalid;
        }

        for (int i = start; i <= end; i++)
        {
            if (!_data.Nonogram[row, i] && !_data.IsAvailable(row, i)) return LineSpace.Invalid;
        }

        return new LineSpace(start, end, sCursor, eCursor);
    }

    private LineSpace DoVerticalMainSpace(int col)
    {
        var list = _data.Nonogram.VerticalLineCollection.AsList(col);
        int sCursor = 0;
        int eCursor = list.Count - 1;

        var buffer = 0;
        var presenceExpected = 0;
        var mustBeUnavailable = false;
        var start = 0;
        while (start < _data.Nonogram.RowCount)
        {
            if (presenceExpected > 0)
            {
                if (!_data.Nonogram[start, col])
                {
                    start = buffer;
                    break;
                }

                presenceExpected--;
                if (presenceExpected == 0)
                {
                    mustBeUnavailable = true;
                    sCursor++;
                }
            }
            else if (mustBeUnavailable)
            {
                if (_data.IsAvailable(start, col)) return LineSpace.Invalid;

                mustBeUnavailable = false;
            }
            else
            {
                if (_data.IsAvailable(start, col)) break;

                if (_data.Nonogram[start, col])
                {
                    presenceExpected = list[sCursor] - 1;
                    if (presenceExpected == 0)
                    {
                        mustBeUnavailable = true;
                        sCursor++;
                    } else buffer = start;
                }
            }

            start++;
        }
        
        presenceExpected = 0;
        mustBeUnavailable = false;
        var end = _data.Nonogram.RowCount - 1;
        while (end >= 0)
        {
            if (presenceExpected > 0)
            {
                if (!_data.Nonogram[end, col])
                {
                    end = buffer;
                    break;
                }

                presenceExpected--;
                if (presenceExpected == 0)
                {
                    mustBeUnavailable = true;
                    eCursor--;
                    if (eCursor < sCursor) return LineSpace.Invalid;
                }
            }
            else if (mustBeUnavailable)
            {
                if (_data.IsAvailable(end, col)) return LineSpace.Invalid;

                mustBeUnavailable = false;
            }
            else
            {
                if (_data.IsAvailable(end, col)) break;

                if (_data.Nonogram[end, col])
                {
                    presenceExpected = list[eCursor] - 1;
                    if (presenceExpected == 0)
                    {
                        mustBeUnavailable = true;
                        eCursor--;
                        if (eCursor < sCursor) return LineSpace.Invalid;
                    } else buffer = end;
                }
            }

            end--;
            if (end < start) return LineSpace.Invalid;
        }

        for (int i = start; i <= end; i++)
        {
            if (!_data.Nonogram[i, col] && !_data.IsAvailable(i, col)) return LineSpace.Invalid;
        }

        return new LineSpace(start, end, sCursor, eCursor);
    }
}

public readonly struct LineSpace
{
    public static LineSpace Invalid = new(-1, -1, -1, -1);
    
    public LineSpace(int start, int end, int valueStart, int valueEnd)
    {
        Start = start;
        End = end;
        ValueStart = valueStart;
        ValueEnd = valueEnd;
    }

    public bool IsInvalid()
    {
        return Start < 0;
    }

    public int GetValueCount() => ValueEnd - ValueStart + 1;

    public int Start { get; }
    public int End { get; }
    public int ValueStart { get; }
    public int ValueEnd { get; }
}