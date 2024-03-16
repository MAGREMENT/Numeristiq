using System.Collections.Generic;
using Model.Utility.BitSets;

namespace Model.Utility;

public class CellsAssociations
{
    private InfiniteBitSet?[,] _sets;

    public CellsAssociations(int rowCount, int colCount)
    {
        _sets = new InfiniteBitSet?[rowCount, colCount];
    }

    public void New(int rowCount, int colCount)
    {
        _sets = new InfiniteBitSet?[rowCount, colCount];
    }
    
    
    public void Merge(Cell one, Cell two)
    {
        var colCount = _sets.GetLength(1);
        
        var i1 = one.Row * colCount + one.Column;
        var i2 = two.Row * colCount + two.Column;

        var set1 = _sets[one.Row, one.Column];
        if (set1 is null)
        {
            set1 = new InfiniteBitSet();
            set1.Set(i1);
        }

        var set2 = _sets[two.Row, two.Column];
        if (set2 is null)
        {
            set2 = new InfiniteBitSet();
            set2.Set(i2);
        }

        set1.Or(set2);
        foreach (var n in set1)
        {
            _sets[n / colCount, n % colCount] = set1;
        }
    }

    public int CountAt(int row, int col) =>_sets[row, col]?.Count ?? 1;

    public bool IsCreatedAt(int row, int col) => _sets[row, col] is not null;

    public InfiniteBitSet SetAt(int row, int col) => _sets[row, col] ?? new InfiniteBitSet();
}