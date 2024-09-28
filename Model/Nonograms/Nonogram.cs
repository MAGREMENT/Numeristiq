using System;
using System.Collections.Generic;
using System.Text;
using Model.Core.BackTracking;
using Model.Utility;

namespace Model.Nonograms;

public class Nonogram : IReadOnlyNonogram
{
    private bool[,] _cells;
    private readonly INonogramLineCollection _horizontalCollection;
    private readonly INonogramLineCollection _verticalCollection;

    public int RowCount => _cells.GetLength(0);
    public int ColumnCount => _cells.GetLength(1);
    public IReadOnlyNonogramLineCollection HorizontalLines => _horizontalCollection;
    public IReadOnlyNonogramLineCollection VerticalLines => _verticalCollection;

    public Nonogram()
    {
        _cells = new bool[0, 0];
        _horizontalCollection = new ListListNonogramLineCollection();
        _verticalCollection = new ListListNonogramLineCollection();
    }

    private Nonogram(bool[,] cells, INonogramLineCollection horizontalCollection, INonogramLineCollection verticalCollection)
    {
        _cells = cells;
        _horizontalCollection = horizontalCollection;
        _verticalCollection = verticalCollection;
    }

    public void Add(IEnumerable<IEnumerable<int>> hValues, IEnumerable<IEnumerable<int>> vValues)
    {
        int rAdded = 0, cAdded = 0;
        foreach (var hv in hValues)
        {
            _horizontalCollection.AddValues(hv);
            rAdded++;
        }

        foreach (var vv in vValues)
        {
            _verticalCollection.AddValues(vv);
            cAdded++;
        }

        ResizeTo(RowCount + rAdded, ColumnCount + cAdded);
    }

    public void SetSizeTo(int rowCount, int colCount)
    {
        var rDiff = rowCount - RowCount;
        var cDiff = colCount - ColumnCount;
        if (rDiff == 0 && cDiff == 0) return;
        
        ResizeTo(rowCount, colCount);
        if (rDiff < 0) _horizontalCollection.RemoveFromEnd(-rDiff);
        else _horizontalCollection.AddEmpty(rDiff);
        if (cDiff < 0) _verticalCollection.RemoveFromEnd(-cDiff);
        else _verticalCollection.AddEmpty(cDiff);
    }
    
    public Nonogram CopyWithoutDichotomy()
    {
        return new Nonogram(new bool[RowCount, ColumnCount], _horizontalCollection.Copy(), _verticalCollection.Copy());
    }

    public Nonogram Copy()
    {
        var n = CopyWithoutDichotomy();
        Array.Copy(_cells, n._cells, _cells.Length);
        return n;
    }

    public bool this[int row, int col]
    {
        get => _cells[row, col];
        set => _cells[row, col] = value;
    }

    public bool IsRowCorrect(int index)
    {
        using var enumerator = _horizontalCollection.AsEnumerable(index).GetEnumerator();

        var remaining = -1;
        for (int col = 0; col < ColumnCount; col++)
        {
            if (_cells[index, col])
            {
                switch (remaining)
                {
                    case 0:
                    case < 0 when !enumerator.MoveNext():
                        return false;
                    case < 0:
                        remaining = enumerator.Current - 1; 
                        continue;
                }
            }
            else if (remaining > 0) return false;

            remaining--;
        }

        return !enumerator.MoveNext();
    }

    public bool IsColumnCorrect(int index)
    {
        using var enumerator = _verticalCollection.AsEnumerable(index).GetEnumerator();

        var remaining = -1;
        for (int row = 0; row < RowCount; row++)
        {
            if (_cells[row, index])
            {
                switch (remaining)
                {
                    case 0:
                    case < 0 when !enumerator.MoveNext():
                        return false;
                    case < 0:
                        remaining = enumerator.Current - 1;
                        continue;
                }
            }
            else if (remaining > 0) return false;

            remaining--;
        }

        return !enumerator.MoveNext();
    }

    public bool IsCorrect()
    {
        for (int row = 0; row < RowCount; row++)
        {
            if (!IsRowCorrect(row)) return false;
        }

        for (int col = 0; col < ColumnCount; col++)
        {
            if (!IsColumnCorrect(col)) return false;
        }

        return true;
    }

    public int GetRowSolutionCount(int row)
    {
        var current = 0;
        for (int c = 0; c < ColumnCount; c++)
        {
            if (this[row, c]) current++;
        }

        return current;
    }

    public int GetColumnSolutionCount(int column)
    {
        var current = 0;
        for (int r = 0; r < RowCount; r++)
        {
            if (this[r, column]) current++;
        }

        return current;
    }

    public override string ToString()
    {
        var maxDepth = 0;
        var maxWidth = 0;

        for (int i = 0; i < _verticalCollection.Count; i++)
        {
            int c = 0;
            foreach (var v in _verticalCollection.AsEnumerable(i))
            {
                maxWidth = Math.Max(maxWidth, v.ToString().Length);
                c++;
            }

            maxDepth = Math.Max(maxDepth, c);
        }

        var maxTotalWidth = 0;

        for (int i = 0; i < _horizontalCollection.Count; i++)
        {
            var c = 0;
            foreach (var v in _horizontalCollection.AsEnumerable(i))
            {
                c += v.ToString().Length + 1;
            }

            maxTotalWidth = Math.Max(maxTotalWidth, c);
        }

        var builder = new StringBuilder();

        for (int vRow = maxDepth - 1; vRow >= 0; vRow--)
        {
            builder.Append(' '.Repeat(maxTotalWidth + 1));

            for (int col = 0; col < ColumnCount; col++)
            {
                var val = _verticalCollection.TryGetValue(col, vRow);
                if (val >= 0) val = _verticalCollection.TryGetValue(col, _verticalCollection.ValueCount(col) - vRow - 1);
                var s = val < 0 ? " " : val.ToString();
                builder.Append(s.FillEvenlyWith(' ', maxWidth) + ' ');
            }

            builder.Append('\n');
        }

        builder.Append(ToStringHorizontalLine(maxTotalWidth, maxWidth) + '\n');
        for (int row = 0; row < RowCount; row++)
        {
            var secondBuilder = new StringBuilder();
            bool first = true;
            foreach (var val in _horizontalCollection.AsEnumerable(row))
            {
                if (first) first = false;
                else secondBuilder.Append(' ');

                secondBuilder.Append(val);
            }

            var valAsString = secondBuilder.ToString();
            builder.Append(' '.Repeat(maxTotalWidth - valAsString.Length) + valAsString + '|');

            for (int col = 0; col < ColumnCount; col++)
            {
                var s = _cells[row, col] ? "0" : " ";
                builder.Append(s.FillEvenlyWith(' ', maxWidth) + '|');
            }

            builder.Append('\n' + ToStringHorizontalLine(maxTotalWidth, maxWidth) + '\n');
        }
        
        return builder.ToString();
    }

    public bool SamePattern(Nonogram n)
    {
        if (n.RowCount != RowCount || n.ColumnCount != ColumnCount) return false;
        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColumnCount; col++)
            {
                if (n[row, col] != this[row, col]) return false;
            }
        }

        return true;
    }

    private string ToStringHorizontalLine(int maxTotalWidth, int maxWidth)
    {
        return ' '.Repeat(maxTotalWidth) + ("+" + '-'.Repeat(maxWidth)).Repeat(ColumnCount) + '+';
    }

    private void ResizeTo(int rowCount, int colCount)
    {
        var buffer = new bool[rowCount, colCount];

        for (int row = 0; row < rowCount && row < RowCount; row++)
        {
            for (int col = 0; col < ColumnCount && col < colCount; col++)
            {
                buffer[row, col] = _cells[row, col];
            }
        }

        _cells = buffer;
    }
}

public interface IReadOnlyNonogram : ICopyable<Nonogram>
{
    int RowCount { get; }
    int ColumnCount { get; }
    IReadOnlyNonogramLineCollection HorizontalLines { get; }
    IReadOnlyNonogramLineCollection VerticalLines { get; }
    bool this[int row, int col] { get; }
    bool IsRowCorrect(int index);
    bool IsColumnCorrect(int index);
    bool IsCorrect();
    int GetRowSolutionCount(int row);
    int GetColumnSolutionCount(int column);
    Nonogram CopyWithoutDichotomy();
}

public interface IReadOnlyNonogramLineCollection
{
    int Count { get; }
    IEnumerable<IEnumerable<int>> Enumerate();
    IEnumerable<int> AsEnumerable(int index);
    IReadOnlyList<int> AsList(int index);
    int TryGetValue(int lineIndex, int valueIndex);
    int ValueCount(int index);
    (int, int) MinValue(int index, int start, int end);
    (int, int) MinValue(int index);
    (int, int) MaxValue(int index, int start, int end);
    (int, int) MaxValue(int index);
    int TotalExpected(int index);
    INonogramLineCollection Copy();
    int NeededSpace(int index, int start, int end);
    public int NeededSpace(int index);
}

public interface INonogramLineCollection : IReadOnlyNonogramLineCollection
{ 
    void SetValues(int index, IEnumerable<int> values);
    void AddValues(IEnumerable<int> values);
    void AddEmpty(int count);
    void RemoveFromEnd(int count);
}

public class ListListNonogramLineCollection : List<List<int>>, INonogramLineCollection
{
    public void SetValues(int index, IEnumerable<int> values)
    {
        var l = this[index];
        l.Clear();
        l.AddRange(values);
    }

    public void AddValues(IEnumerable<int> values)
    { 
        Add(new List<int>(values));
    }

    public void AddEmpty(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Add(new List<int>());
        }
    }

    public void RemoveFromEnd(int count)
    {
        RemoveRange(Count - count, count);
    }

    public IEnumerable<int> AsEnumerable(int index) => this[index];

    public IReadOnlyList<int> AsList(int index) => this[index];

    public int TryGetValue(int lineIndex, int valueIndex)
    {
        if (lineIndex < 0 || lineIndex >= Count) return -1;

        var l = this[lineIndex];
        if (valueIndex < 0 || valueIndex >= l.Count) return -1;

        return l[valueIndex];
    }

    public int ValueCount(int index)
    {
        return this[index].Count;
    }

    public (int, int) MinValue(int index, int start, int end)
    {
        var l = this[index];
        
        var v = l[start];
        var i = start;
        for (int n = start + 1; n <= end; n++)
        {
            if (l[n] < v)
            {
                v = l[n];
                i = n;
            }
        }

        return (i, v);
    }

    public (int, int) MinValue(int index) => MinValue(index, 0, this[index].Count - 1);
    
    public (int, int) MaxValue(int index, int start, int end)
    {
        var l = this[index];
        
        var v = l[start];
        var i = start;
        for (int n = start + 1; n <= end; n++)
        {
            if (l[n] > v)
            {
                v = l[n];
                i = n;
            }
        }

        return (i, v);
    }

    public (int, int) MaxValue(int index) => MaxValue(index, 0, this[index].Count - 1);

    public int TotalExpected(int index)
    {
        var total = 0;
        foreach (var val in this[index]) total += val;
        return total;
    }

    public INonogramLineCollection Copy()
    {
        var buffer = new ListListNonogramLineCollection();
        foreach (var l in this)
        {
            buffer.AddValues(l);
        }

        return buffer;
    }

    public int NeededSpace(int index, int start, int end)
    {
        var l = this[index];
        var result = 0;
        for (int i = start; i <= end; i++)
        {
            if (result > 0) result++;
            result += l[i];
        }

        return result;    
    }

    public int NeededSpace(int index)
    {
        return NeededSpace(index, 0, this[index].Count - 1);
    }

    public IEnumerable<IEnumerable<int>> Enumerate()
    {
        return this;
    }
}