using System;
using System.Collections.Generic;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Nonograms.Solver;

public class NonogramPreComputer
{
    private readonly INonogramSolverData _data;
    
    private MainSpace[] _horizontalMainSpace = Array.Empty<MainSpace>();
    private MainSpace[] _verticalMainSpace = Array.Empty<MainSpace>();
    private readonly InfiniteBitSet _hmsHits = new();
    private readonly InfiniteBitSet _vmsHits = new();

    private List<ValueSpace>?[] _horizontalValueSpaces = Array.Empty<List<ValueSpace>>();
    private List<ValueSpace>?[] _verticalValueSpaces = Array.Empty<List<ValueSpace>>();

    public NonogramPreComputer(INonogramSolverData data)
    {
        _data = data;
    }

    public void AdaptToNewSize(int rowCount, int colCount)
    {
        _horizontalMainSpace = new MainSpace[rowCount];
        _horizontalValueSpaces = new List<ValueSpace>?[rowCount];
        
        _verticalMainSpace = new MainSpace[colCount];
        _verticalValueSpaces = new List<ValueSpace>?[colCount];
    }

    public MainSpace HorizontalMainSpace(int row)
    {
        if (!_hmsHits.Contains(row)) _horizontalMainSpace[row] = DoHorizontalMainSpace(row);
        return _horizontalMainSpace[row];
    }

    public MainSpace VerticalMainSpace(int col)
    {
        if (!_vmsHits.Contains(col)) _verticalMainSpace[col] = DoVerticalMainSpace(col);
        return _verticalMainSpace[col];
    }

    public IReadOnlyList<ValueSpace> HorizontalValueSpaces(int row)
    {
        _horizontalValueSpaces[row] ??= DoHorizontalValueSpaces(row);
        return _horizontalValueSpaces[row]!;
    }

    public IReadOnlyList<ValueSpace> VerticalValueSpaces(int col)
    {
        _verticalValueSpaces[col] ??= DoVerticalValueSpaces(col);
        return _verticalValueSpaces[col]!;
    }

    public void Reset()
    {
        _hmsHits.Clear();
        _vmsHits.Clear();
        Array.Fill(_horizontalValueSpaces, null);
        Array.Fill(_verticalValueSpaces, null);
    }

    private List<ValueSpace> DoHorizontalValueSpaces(int row)
    {
        var list = new List<ValueSpace>();
        var main = HorizontalMainSpace(row);
        if (main.IsInvalid()) return list;

        var spaceBefore = 0;
        var spaceAfter = _data.Nonogram.HorizontalLineCollection.NeededSpace(row, main.FirstValueIndex, main.LastValueIndex);
        for (int i = main.FirstValueIndex; i <= main.LastValueIndex; i++)
        {
            var v = _data.Nonogram.HorizontalLineCollection.TryGetValue(row, i);
            spaceAfter -= Math.Min(v + 1, spaceAfter);
            
            var start = main.Start + spaceBefore;
            var end = main.End - spaceAfter;

            for (int pos = start; pos <= end; pos++)
            {
                if (_data.IsAvailable(row, pos)) continue;

                if (_data.Nonogram[row, pos])
                {
                    var endPos = pos;
                    while (_data.Nonogram[row, endPos + 1]) endPos++;

                    var length = endPos - pos + 1;
                    if (pos - start + 1 < v) start = Math.Max(pos - v + length, start);
                    if (end - endPos + 1 < v) end = Math.Min(endPos + v - length, end);
                }
                else
                {
                    if (pos - start < v) start = pos + 1;
                    if (end - pos < v) end = pos - 1;
                }
            }

            list.Add(new ValueSpace(start, end, v));
            spaceBefore += v + 1;
        }

        return list;
    }
    
    private List<ValueSpace> DoVerticalValueSpaces(int col)
    {
        var list = new List<ValueSpace>();
        var main = VerticalMainSpace(col);
        if (main.IsInvalid()) return list;

        var spaceBefore = 0;
        var spaceAfter = _data.Nonogram.VerticalLineCollection.NeededSpace(col, main.FirstValueIndex, main.LastValueIndex);
        for (int i = main.FirstValueIndex; i <= main.LastValueIndex; i++)
        {
            var v = _data.Nonogram.VerticalLineCollection.TryGetValue(col, i);
            spaceAfter -= Math.Min(v + 1, spaceAfter);
            
            var start = main.Start + spaceBefore;
            var end = main.End - spaceAfter;

            for (int pos = start; pos <= end; pos++)
            {
                if (_data.IsAvailable(pos, col)) continue;

                if (_data.Nonogram[pos, col])
                {
                    var endPos = pos;
                    while (_data.Nonogram[endPos + 1, col]) endPos++;

                    var length = endPos - pos + 1;
                    if (pos - start + 1 < v) start = Math.Max(pos - v + length, start);
                    if (end - endPos + 1 < v) end = Math.Min(endPos + v - length, end);
                }
                else
                {
                    if (pos - start < v) start = pos + 1;
                    if (end - pos < v) end = pos - 1;
                }
            }

            list.Add(new ValueSpace(start, end, v));
            spaceBefore += v + 1;
        }

        return list;
    }
    
    private MainSpace DoHorizontalMainSpace(int row)
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
                if (_data.IsAvailable(row, start)) return MainSpace.Invalid;

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
                    if (eCursor < sCursor) return MainSpace.Invalid;
                }
            }
            else if (mustBeUnavailable)
            {
                if (_data.IsAvailable(row, end)) return MainSpace.Invalid;

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
                        if (eCursor < sCursor) return MainSpace.Invalid;
                    } else buffer = start;
                }
            }

            end--;
            if (end < start) return MainSpace.Invalid;
        }

        return new MainSpace(start, end, sCursor, eCursor);
    }

    private MainSpace DoVerticalMainSpace(int col)
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
                if (_data.IsAvailable(start, col)) return MainSpace.Invalid;

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
                    if (eCursor < sCursor) return MainSpace.Invalid;
                }
            }
            else if (mustBeUnavailable)
            {
                if (_data.IsAvailable(end, col)) return MainSpace.Invalid;

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
                        if (eCursor < sCursor) return MainSpace.Invalid;
                    } else buffer = end;
                }
            }

            end--;
            if (end < start) return MainSpace.Invalid;
        }

        return new MainSpace(start, end, sCursor, eCursor);
    }
}

public readonly struct ValueSpace
{
    public ValueSpace(int start, int end, int value)
    {
        Start = start;
        End = end;
        Value = value;
    }

    public bool IsValid() => End - Start + 1 >= Value;
    public int GetLength() => End - Start + 1;

    /// <summary>
    /// Inclusive
    /// </summary>
    public int Start { get; }
    /// <summary>
    /// Inclusive
    /// </summary>
    public int End { get; }
    public int Value { get; }
}

public readonly struct MainSpace
{
    public static MainSpace Invalid = new(-1, -1, -1, -1);
    
    public MainSpace(int start, int end, int firstValueIndex, int lastValueIndex)
    {
        Start = start;
        End = end;
        FirstValueIndex = firstValueIndex;
        LastValueIndex = lastValueIndex;
    }

    public IEnumerable<Cell> EnumerateCells(Orientation orientation, int unit)
    {
        for (int i = Start; i <= End; i++)
        {
            yield return orientation == Orientation.Vertical ? new Cell(i, unit) : new Cell(unit, i);
        }
    }

    public bool IsInvalid()
    {
        return Start < 0;
    }

    public int GetValueCount() => LastValueIndex - FirstValueIndex + 1;

    /// <summary>
    /// Inclusive
    /// </summary>
    public int Start { get; }
    /// <summary>
    /// Inclusive
    /// </summary>
    public int End { get; }
    public int FirstValueIndex { get; }
    public int LastValueIndex { get; }
}