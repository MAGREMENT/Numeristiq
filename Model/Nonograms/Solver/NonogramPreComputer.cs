using System;
using System.Collections.Generic;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Nonograms.Solver;

public class NonogramPreComputer
{
    private readonly INonogramSolverData _data;
    
    private MultiValueSpace[] _horizontalMainSpace = Array.Empty<MultiValueSpace>();
    private MultiValueSpace[] _verticalMainSpace = Array.Empty<MultiValueSpace>();
    private readonly InfiniteBitSet _hrvHits = new();
    private readonly InfiniteBitSet _vrvHits = new();

    private ValueSpaceCollection?[] _horizontalValueSpaces = Array.Empty<ValueSpaceCollection>();
    private ValueSpaceCollection?[] _verticalValueSpaces = Array.Empty<ValueSpaceCollection>();

    public NonogramPreComputer(INonogramSolverData data)
    {
        _data = data;
    }

    public void AdaptToNewSize(int rowCount, int colCount)
    {
        _horizontalMainSpace = new MultiValueSpace[rowCount];
        _horizontalValueSpaces = new ValueSpaceCollection?[rowCount];
        
        _verticalMainSpace = new MultiValueSpace[colCount];
        _verticalValueSpaces = new ValueSpaceCollection?[colCount];
    }

    public MultiValueSpace HorizontalRemainingValuesSpace(int row)
    {
        if (!_hrvHits.Contains(row)) _horizontalMainSpace[row] = DoHorizontalRemainingValuesSpace(row);
        return _horizontalMainSpace[row];
    }

    public MultiValueSpace VerticalRemainingValuesSpace(int col)
    {
        if (!_vrvHits.Contains(col)) _verticalMainSpace[col] = DoVerticalRemainingValuesSpace(col);
        return _verticalMainSpace[col];
    }

    public IReadOnlyValueSpaceCollection HorizontalValueSpaces(int row)
    {
        _horizontalValueSpaces[row] ??= DoHorizontalValueSpaces(row);
        return _horizontalValueSpaces[row]!;
    }

    public IReadOnlyValueSpaceCollection VerticalValueSpaces(int col)
    {
        _verticalValueSpaces[col] ??= DoVerticalValueSpaces(col);
        return _verticalValueSpaces[col]!;
    }

    public void Reset()
    {
        _hrvHits.Clear();
        _vrvHits.Clear();
        Array.Fill(_horizontalValueSpaces, null);
        Array.Fill(_verticalValueSpaces, null);
    }

    private ValueSpaceCollection DoHorizontalValueSpaces(int row)
    {
        var list = new ValueSpaceCollection();
        var main = HorizontalRemainingValuesSpace(row);
        if (main.IsInvalid()) return list;
        
        var spaceBefore = 0;
        var spaceAfter = _data.Nonogram.HorizontalLineCollection.NeededSpace(row, main.FirstValueIndex, main.LastValueIndex);
        for (int i = main.FirstValueIndex; i <= main.LastValueIndex; i++)
        {
            var v = _data.Nonogram.HorizontalLineCollection.TryGetValue(row, i);
            spaceAfter -= Math.Min(v, spaceAfter);

            int start, end;
            if (i == main.FirstValueIndex && _data.Nonogram[row, main.Start])
            {
                start = main.Start;
                end = main.Start + v - 1;
            }
            else if (i == main.LastValueIndex && _data.Nonogram[row, main.End])
            {
                end = main.End;
                start = main.End - v + 1;
            }
            else
            {
                start = main.Start + spaceBefore;
                end = main.End - spaceAfter;

                for (int pos = start; pos <= end; pos++)
                {
                    if (_data.IsAvailable(row, pos)) continue;

                    if (_data.Nonogram[row, pos])
                    {
                        var endPos = pos;
                        while (endPos < _data.Nonogram.ColumnCount - 1 && _data.Nonogram[row, endPos + 1]) endPos++;

                        var length = endPos - pos + 1;
                        if (pos - start < v + 1) start = Math.Max(pos - v + length, start);
                        if (end - endPos < v + 1) end = Math.Min(endPos + v - length, end);
                    }
                    else
                    {
                        if (pos - start < v) start = pos + 1;
                        if (end - pos < v) end = pos - 1;
                    }
                }
            }

            list.Add(new ValueSpace(start, end, v));
            spaceBefore += v + 1;
            spaceAfter--;
        }

        list.FirstValueIndex = main.FirstValueIndex;
        list.LastValueIndex = main.LastValueIndex;
        return list;
    }
    
    private ValueSpaceCollection DoVerticalValueSpaces(int col)
    {
        var list = new ValueSpaceCollection();
        var main = VerticalRemainingValuesSpace(col);
        if (main.IsInvalid()) return list;

        var spaceBefore = 0;
        var spaceAfter = _data.Nonogram.VerticalLineCollection.NeededSpace(col, main.FirstValueIndex, main.LastValueIndex);
        for (int i = main.FirstValueIndex; i <= main.LastValueIndex; i++)
        {
            var v = _data.Nonogram.VerticalLineCollection.TryGetValue(col, i);
            spaceAfter -= Math.Min(v, spaceAfter);
            
            int start, end;
            if (i == main.FirstValueIndex && _data.Nonogram[main.Start, col])
            {
                start = main.Start;
                end = main.Start + v - 1;
            }
            else if (i == main.LastValueIndex && _data.Nonogram[main.End, col])
            {
                end = main.End;
                start = main.End - v + 1;
            }
            else
            {
                start = main.Start + spaceBefore;
                end = main.End - spaceAfter;

                for (int pos = start; pos <= end; pos++)
                {
                    if (_data.IsAvailable(pos, col)) continue;

                    if (_data.Nonogram[pos, col])
                    {
                        var endPos = pos;
                        while (endPos < _data.Nonogram.RowCount - 1 && _data.Nonogram[endPos + 1, col]) endPos++;

                        var length = endPos - pos + 1;
                        if (pos - start < v + 1) start = Math.Max(pos - v + length, start);
                        if (end - endPos < v + 1) end = Math.Min(endPos + v - length, end);
                    }
                    else
                    {
                        if (pos - start < v) start = pos + 1;
                        if (end - pos < v) end = pos - 1;
                    }
                }
            }

            list.Add(new ValueSpace(start, end, v));
            spaceBefore += v + 1;
            spaceAfter--;
        }

        list.FirstValueIndex = main.FirstValueIndex;
        list.LastValueIndex = main.LastValueIndex;
        return list;
    }
    
    private MultiValueSpace DoHorizontalRemainingValuesSpace(int row)
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

                if (--presenceExpected == 0) mustBeUnavailable = true;
            }
            else if (mustBeUnavailable)
            {
                if (_data.IsAvailable(row, start))
                {
                    start = buffer;
                    break;
                }

                sCursor++;
                mustBeUnavailable = false;
            }
            else
            {
                if (_data.IsAvailable(row, start)) break;

                if (_data.Nonogram[row, start])
                {
                    presenceExpected = list[sCursor] - 1;
                    buffer = start;
                    
                    if (presenceExpected == 0) mustBeUnavailable = true;
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

                if (--presenceExpected == 0) mustBeUnavailable = true;
            }
            else if (mustBeUnavailable)
            {
                if (_data.IsAvailable(row, end))
                {
                    end = buffer;
                    break;
                }

                eCursor--;
                if (eCursor < sCursor) return MultiValueSpace.Invalid;
                mustBeUnavailable = false;
            }
            else
            {
                if (_data.IsAvailable(row, end)) break;

                if (_data.Nonogram[row, end])
                {
                    presenceExpected = list[eCursor] - 1;
                    buffer = end;
                    
                    if (presenceExpected == 0) mustBeUnavailable = true;
                }
            }

            end--;
            if (end < start) return MultiValueSpace.Invalid;
        }

        return new MultiValueSpace(start, end, sCursor, eCursor);
    }

    private MultiValueSpace DoVerticalRemainingValuesSpace(int col)
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
                
                if (--presenceExpected == 0) mustBeUnavailable = true;
            }
            else if (mustBeUnavailable)
            {
                if (_data.IsAvailable(start, col))
                {
                    start = buffer;
                    break;
                }

                sCursor++;
                mustBeUnavailable = false;
            }
            else
            {
                if (_data.IsAvailable(start, col)) break;

                if (_data.Nonogram[start, col])
                {
                    presenceExpected = list[sCursor] - 1;
                    buffer = start;
                    
                    if (presenceExpected == 0) mustBeUnavailable = true;
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

                if (--presenceExpected == 0) mustBeUnavailable = true;
            }
            else if (mustBeUnavailable)
            {
                if (_data.IsAvailable(end, col))
                {
                    end = buffer;
                    break;
                }

                eCursor--;
                if (eCursor < sCursor) return MultiValueSpace.Invalid;
                mustBeUnavailable = false;
            }
            else
            {
                if (_data.IsAvailable(end, col)) break;

                if (_data.Nonogram[end, col])
                {
                    presenceExpected = list[eCursor] - 1;
                    buffer = end;
                    
                    if (presenceExpected == 0) mustBeUnavailable = true;
                }
            }

            end--;
            if (end < start) return MultiValueSpace.Invalid;
        }

        return new MultiValueSpace(start, end, sCursor, eCursor);
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

    public bool IsCompleted(IReadOnlyNonogram nonogram, int n, Orientation orientation)
    {
        if (GetLength() != Value) return false;
        
        if (orientation == Orientation.Horizontal)
        {
            for (int c = Start; c <= End; c++)
            {
                if (!nonogram[n, c]) return false;
            }

            return true;
        }
        
        for (int r = Start; r <= End; r++)
        {
            if (!nonogram[r, n]) return false;
        }

        return true;
    }

    /// <summary>
    /// Inclusive
    /// </summary>
    public int Start { get; }
    /// <summary>
    /// Inclusive
    /// </summary>
    public int End { get; }
    public int Value { get; }

    public override bool Equals(object? obj)
    {
        return obj is ValueSpace vs && vs == this;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End, Value);
    }

    public override string ToString()
    {
        return $"{Value} : {Start} -> {End}";
    }

    public static bool operator ==(ValueSpace left, ValueSpace right)
    {
        return left.Start == right.Start && left.End == right.End && left.Value == right.Value;
    }

    public static bool operator !=(ValueSpace left, ValueSpace right)
    {
        return !(left == right);
    }
}

public readonly struct MultiValueSpace
{
    public static MultiValueSpace Invalid = new(-1, -1, -1, -1);
    
    public MultiValueSpace(int start, int end, int firstValueIndex, int lastValueIndex)
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

public interface IReadOnlyValueSpaceCollection : IReadOnlyList<ValueSpace>
{
    public int FirstValueIndex { get; }
    public int LastValueIndex { get; }

    public int MinValueForSpace(int start, int end);
    public int MaxValueForSpace(int start, int end);
}

public class ValueSpaceCollection : List<ValueSpace>, IReadOnlyValueSpaceCollection
{
    public int FirstValueIndex { get; set; }
    public int LastValueIndex { get; set; }

    public int MinValueForSpace(int start, int end)
    {
        return 0; //TODO
    }

    public int MaxValueForSpace(int start, int end)
    {
        return 0; //TODO
    }
}