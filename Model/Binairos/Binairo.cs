using System;
using System.Text;
using Model.Core.BackTracking;
using Model.Core.Generators;
using Model.Utility;

namespace Model.Binairos;

public class Binairo : IReadOnlyBinairo, ICopyable<Binairo>, ICellsAndDigitsPuzzle
{
    private readonly int[,] _cells;
    private readonly ReadOnlyBinairoUnitBitSet[] _rowSets;
    private readonly ReadOnlyBinairoUnitBitSet[] _colSets;

    public int RowCount => _rowSets.Length;
    public int ColumnCount => _colSets.Length;

    public Binairo(int rowCount, int colCount)
    {
        _cells = new int[rowCount, colCount];
        _rowSets = new ReadOnlyBinairoUnitBitSet[rowCount];
        _colSets = new ReadOnlyBinairoUnitBitSet[colCount];
    }

    public Binairo()
    {
        _cells = new int[0, 0];
        _rowSets = Array.Empty<ReadOnlyBinairoUnitBitSet>();
        _colSets = Array.Empty<ReadOnlyBinairoUnitBitSet>();
    }

    private Binairo(int[,] cells, ReadOnlyBinairoUnitBitSet[] rowSets, ReadOnlyBinairoUnitBitSet[] colSets)
    {
        _cells = cells;
        _rowSets = rowSets;
        _colSets = colSets;
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
            total += set.GetTotalCount();
        }

        return total;
    }

    public bool SamePattern(IReadOnlyBinairo binairo)
    {
        if (binairo.RowCount != RowCount || binairo.ColumnCount != ColumnCount) return false;

        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColumnCount; col++)
            {
                if (binairo[row, col] != this[row, col]) return false;
            }
        }

        return true;
    }

    public Binairo Copy()
    {
        var cells = new int[RowCount, ColumnCount];
        var rowSets = new ReadOnlyBinairoUnitBitSet[RowCount];
        var colSets = new ReadOnlyBinairoUnitBitSet[ColumnCount];

        Array.Copy(_cells, cells, _cells.Length);
        Array.Copy(_rowSets, rowSets, _rowSets.Length);
        Array.Copy(_colSets, colSets, _colSets.Length);

        return new Binairo(cells, rowSets, colSets);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Binairo b) return false;
        if (!b.SamePattern(this)) return false;

        for (int i = 0; i < _rowSets.Length; i++)
        {
            if (b._rowSets[i] != _rowSets[i]) return false;
        }
        
        for (int i = 0; i < _colSets.Length; i++)
        {
            if (b._colSets[i] != _colSets[i]) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_cells.GetHashCode(), _rowSets.GetHashCode(), _colSets.GetHashCode());
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
    public int OnesCount { get; }
    public int TwosCount { get; }

    private ReadOnlyBinairoUnitBitSet(ulong bits, int onesCount, int twosCount)
    {
        _bits = bits;
        OnesCount = onesCount;
        TwosCount = twosCount;
    }

    public int this[int index] => (int)((_bits >> (index * 2)) & 3ul);

    public bool Contains(ReadOnlyBinairoUnitBitSet set) => (_bits | set._bits) == _bits;

    public int GetTotalCount() => OnesCount + TwosCount;
    
    public override bool Equals(object? obj)
    {
        return obj is ReadOnlyBinairoUnitBitSet other && other == this;
    }

    public override int GetHashCode()
    {
        return (int)_bits;
    }

    public ReadOnlyBinairoUnitBitSet Add(int index, int value)
    {
        switch (value)
        {
            case 0 : return this - index;
            case 1 : 
                var added1 = this[index] == 0;
                return added1 ? new ReadOnlyBinairoUnitBitSet(_bits | ((ulong)value << (index * 2)), 
                    OnesCount + 1, TwosCount) : this;
            case 2 :
                var added2 = this[index] == 0;
                return added2 ? new ReadOnlyBinairoUnitBitSet(_bits | ((ulong)value << (index * 2)), 
                    OnesCount, TwosCount + 1) : this;
            default: return this;
        }
    }

    public static ReadOnlyBinairoUnitBitSet operator -(ReadOnlyBinairoUnitBitSet set, int index)
    {
        var removed = set[index];
        return removed switch
        {
            1 => new ReadOnlyBinairoUnitBitSet(set._bits & ~(3ul << (index * 2)),
                set.OnesCount - 1, set.TwosCount),
            2 => new ReadOnlyBinairoUnitBitSet(set._bits & ~(3ul << (index * 2)),
                set.OnesCount, set.TwosCount - 1),
            _ => set
        };
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

    public bool SamePattern(IReadOnlyBinairo binairo);
}