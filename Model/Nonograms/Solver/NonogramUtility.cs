using System;
using System.Collections.Generic;
using Model.Core.BackTracking;
using Model.Utility;

namespace Model.Nonograms.Solver;

public static class NonogramUtility
{
    public static ValueSpaceCollection HorizontalValueSpaces(IReadOnlyNonogram nonogram, IAvailabilityChecker checker,
        MultiValueSpace main, int row)
    {
        var list = new ValueSpaceCollection();
        if (main.IsInvalid()) return list;
        
        var spaceBefore = 0;
        var spaceAfter = nonogram.HorizontalLines.NeededSpace(row, main.FirstValueIndex, main.LastValueIndex);
        for (int i = main.FirstValueIndex; i <= main.LastValueIndex; i++)
        {
            var v = nonogram.HorizontalLines.TryGetValue(row, i);
            spaceAfter -= Math.Min(v, spaceAfter);

            int start, end;
            if (i == main.FirstValueIndex && nonogram[row, main.Start])
            {
                start = main.Start;
                end = main.Start + v - 1;
            }
            else if (i == main.LastValueIndex && nonogram[row, main.End])
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
                    if (checker.IsAvailable(row, pos)) continue;

                    if (nonogram[row, pos])
                    {
                        var endPos = pos;
                        while (endPos < nonogram.ColumnCount - 1 && nonogram[row, endPos + 1]) endPos++;

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
    
    public static ValueSpaceCollection VerticalValueSpaces(IReadOnlyNonogram nonogram, IAvailabilityChecker checker,
        MultiValueSpace main, int col)
    {
        var list = new ValueSpaceCollection();
        if (main.IsInvalid()) return list;

        var spaceBefore = 0;
        var spaceAfter = nonogram.VerticalLines.NeededSpace(col, main.FirstValueIndex, main.LastValueIndex);
        for (int i = main.FirstValueIndex; i <= main.LastValueIndex; i++)
        {
            var v = nonogram.VerticalLines.TryGetValue(col, i);
            spaceAfter -= Math.Min(v, spaceAfter);
            
            int start, end;
            if (i == main.FirstValueIndex && nonogram[main.Start, col])
            {
                start = main.Start;
                end = main.Start + v - 1;
            }
            else if (i == main.LastValueIndex && nonogram[main.End, col])
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
                    if (checker.IsAvailable(pos, col)) continue;

                    if (nonogram[pos, col])
                    {
                        var endPos = pos;
                        while (endPos < nonogram.RowCount - 1 && nonogram[endPos + 1, col]) endPos++;

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
    
    public static MultiValueSpace HorizontalRemainingValuesSpace(IReadOnlyNonogram nonogram,
        IAvailabilityChecker checker, int row)
    {
        var list = nonogram.HorizontalLines.AsList(row);
        int sCursor = 0;
        int eCursor = list.Count - 1;

        var buffer = 0;
        var presenceExpected = 0;
        var mustBeUnavailable = false;
        var start = 0;
        while (start < nonogram.ColumnCount)
        {
            if (presenceExpected > 0)
            {
                if (!nonogram[row, start])
                {
                    start = buffer;
                    break;
                }

                if (--presenceExpected == 0) mustBeUnavailable = true;
            }
            else if (mustBeUnavailable)
            {
                if (checker.IsAvailable(row, start))
                {
                    start = buffer;
                    break;
                }

                sCursor++;
                mustBeUnavailable = false;
            }
            else
            {
                if (checker.IsAvailable(row, start)) break;

                if (nonogram[row, start])
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
        var end = nonogram.ColumnCount - 1;
        while (end >= 0)
        {
            if (presenceExpected > 0)
            {
                if (!nonogram[row, end])
                {
                    end = buffer;
                    break;
                }

                if (--presenceExpected == 0) mustBeUnavailable = true;
            }
            else if (mustBeUnavailable)
            {
                if (checker.IsAvailable(row, end))
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
                if (checker.IsAvailable(row, end)) break;

                if (nonogram[row, end])
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

    public static MultiValueSpace VerticalRemainingValuesSpace(IReadOnlyNonogram nonogram,
        IAvailabilityChecker checker, int col)
    {
        var list = nonogram.VerticalLines.AsList(col);
        int sCursor = 0;
        int eCursor = list.Count - 1;

        var buffer = 0;
        var presenceExpected = 0;
        var mustBeUnavailable = false;
        var start = 0;
        while (start < nonogram.RowCount)
        {
            if (presenceExpected > 0)
            {
                if (!nonogram[start, col])
                {
                    start = buffer;
                    break;
                }
                
                if (--presenceExpected == 0) mustBeUnavailable = true;
            }
            else if (mustBeUnavailable)
            {
                if (checker.IsAvailable(start, col))
                {
                    start = buffer;
                    break;
                }

                sCursor++;
                mustBeUnavailable = false;
            }
            else
            {
                if (checker.IsAvailable(start, col)) break;

                if (nonogram[start, col])
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
        var end = nonogram.RowCount - 1;
        while (end >= 0)
        {
            if (presenceExpected > 0)
            {
                if (!nonogram[end, col])
                {
                    end = buffer;
                    break;
                }

                if (--presenceExpected == 0) mustBeUnavailable = true;
            }
            else if (mustBeUnavailable)
            {
                if (checker.IsAvailable(end, col))
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
                if (checker.IsAvailable(end, col)) break;

                if (nonogram[end, col])
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
}

public class ValueSpaceCollection : List<ValueSpace>, IReadOnlyValueSpaceCollection
{
    public int FirstValueIndex { get; set; }
    public int LastValueIndex { get; set; }
}