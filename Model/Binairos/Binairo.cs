using System;
using System.Text;
using Model.Utility;

namespace Model.Binairos;

public class Binairo : IReadOnlyBinairo
{
    private readonly int[,] _cells;
    private readonly ReadOnlyBinairoUnitBitSet[] _rowSets;
    private readonly ReadOnlyBinairoUnitBitSet[] _colSets;
    
    public int RowCount { get; }
    public int ColumnCount { get; }

    public Binairo(int rowCount, int colCount)
    {
        RowCount = rowCount;
        ColumnCount = colCount;
        _cells = new int[rowCount, colCount];
        _rowSets = new ReadOnlyBinairoUnitBitSet[rowCount];
        _colSets = new ReadOnlyBinairoUnitBitSet[colCount];
    }

    public Binairo()
    {
        RowCount = 0;
        ColumnCount = 0;
        _cells = new int[0, 0];
        _rowSets = Array.Empty<ReadOnlyBinairoUnitBitSet>();
        _colSets = Array.Empty<ReadOnlyBinairoUnitBitSet>();
    }

    public int this[int row, int col]
    {
        get => _cells[row, col];
        set
        {
            _cells[row, col] = value;
            _rowSets[row] = _rowSets[row].Add(col, value);
            _colSets[col] = _colSets[col].Add(row, value);
        } 
    }

    public ReadOnlyBinairoUnitBitSet RowSetAt(int row) => _rowSets[row];
    public ReadOnlyBinairoUnitBitSet ColumnSetAt(int col) => _colSets[col];

    public bool IsCorrect()
    {
        for (int row = 0; row < RowCount; row++)
        {
            var streak = 0;
            int last = 0;
            int oneCount = 0;
            int twoCount = 0;

            for (int col = 0; col < ColumnCount; col++)
            {
                var number = _cells[row, col];
                switch (number)
                {
                    case 0 : return false;
                    case 1 : oneCount++;
                        break;
                    case 2 : twoCount++;
                        break;
                    default: throw new Exception();
                }

                if (number == last)
                {
                    streak++;
                    if (streak > 2) return false;
                }
                else
                {
                    streak = 1;
                    last = number;
                }
            }

            if (oneCount != twoCount) return false;
        }
        
        for (int col = 0; col < ColumnCount; col++)
        {
            var streak = 0;
            int last = 0;
            int oneCount = 0;
            int twoCount = 0;

            for (int row = 0; row < RowCount; row++)
            {
                var number = _cells[row, col];
                switch (number)
                {
                    case 0 : return false;
                    case 1 : oneCount++;
                        break;
                    case 2 : twoCount++;
                        break;
                    default: throw new Exception();
                }

                if (number == last)
                {
                    streak++;
                    if (streak > 2) return false;
                }
                else
                {
                    streak = 1;
                    last = number;
                }
            }

            if (oneCount != twoCount) return false;
        }

        return IsCorrect(_rowSets) && IsCorrect(_colSets);
    }

    public int GetSolutionCount()
    {
        var total = 0;
        foreach (var set in _rowSets)
        {
            total += set.Count;
        }

        return total;
    }

    public override string ToString()
    {
        var builder = new StringBuilder(ColumnLineString());
        for (int row = 0; row < RowCount; row++)
        {
            builder.Append('\n');
            builder.Append('|');
            for (int col = 0; col < ColumnCount; col++)
            {
                var n = _cells[row, col];
                builder.Append(n == 0 ? "  " : ' ' + (n - 1).ToString());
                builder.Append(" |");
            }
            
            builder.Append('\n');
            builder.Append(ColumnLineString());
        }

        return builder.ToString();
    }

    private string ColumnLineString() => '+' + "---+".Repeat(ColumnCount);

    private static bool IsCorrect(ReadOnlyBinairoUnitBitSet[] sets)
    {
        for (int i = 0; i < sets.Length - 1; i++)
        {
            for (int j = i + 1; j < sets.Length; j++)
            {
                if (i == j) return false;
            }
        }

        return true;
    }
}

public readonly struct ReadOnlyBinairoUnitBitSet
{
    private readonly ulong _bits;
    public int Count { get; }

    private ReadOnlyBinairoUnitBitSet(ulong bits, int count)
    {
        _bits = bits;
        Count = count;
    }

    public int this[int index] => (int)((_bits >> (index * 2)) & 3);
    
    public override bool Equals(object? obj)
    {
        return obj is ReadOnlyBinairoUnitBitSet other && other == this;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_bits, Count);
    }

    public ReadOnlyBinairoUnitBitSet Add(int index, int value)
    {
        if (value == 0) return this - index;
        
        var added = this[index] == 0;
        return new ReadOnlyBinairoUnitBitSet(_bits | ((ulong)value << (index * 2)), Count + (added ? 1 : 0));
    }

    public static ReadOnlyBinairoUnitBitSet operator -(ReadOnlyBinairoUnitBitSet set, int index)
    {
        var removed = set[index] != 0;
        return new ReadOnlyBinairoUnitBitSet(set._bits & ~(1ul << (index * 2)), set.Count - (removed ? 1 : 0));
    }

    public static bool operator ==(ReadOnlyBinairoUnitBitSet set1, ReadOnlyBinairoUnitBitSet set2)
    {
        return set1._bits == set2._bits;
    }

    public static bool operator !=(ReadOnlyBinairoUnitBitSet set1, ReadOnlyBinairoUnitBitSet set2)
    {
        return set1._bits != set2._bits;
    }
}

public interface IReadOnlyBinairo
{
    public int RowCount { get; }
    public int ColumnCount { get; }

    public ReadOnlyBinairoUnitBitSet RowSetAt(int row);
    public ReadOnlyBinairoUnitBitSet ColumnSetAt(int col);
    
    public int this[int row, int col] { get; }

    public bool IsCorrect();
    public int GetSolutionCount();
}