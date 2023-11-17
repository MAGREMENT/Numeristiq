using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using Global;

namespace Model.Solver.Strategies.SetEquivalence.Searchers;

public static class SearcherUtils
{
    public static readonly HouseFiller[] Fillers =
    {
        new RowFiller(0),
        new RowFiller(1),
        new RowFiller(2),
        new RowFiller(3),
        new RowFiller(4),
        new RowFiller(5),
        new RowFiller(6),
        new RowFiller(7),
        new RowFiller(8),
        new ColumnFiller(0),
        new ColumnFiller(1),
        new ColumnFiller(2),
        new ColumnFiller(3),
        new ColumnFiller(4),
        new ColumnFiller(5),
        new ColumnFiller(6),
        new ColumnFiller(7),
        new ColumnFiller(8),
        new MiniGridFiller(0, 0),
        new MiniGridFiller(0, 1),
        new MiniGridFiller(0, 2),
        new MiniGridFiller(1, 0),
        new MiniGridFiller(1, 1),
        new MiniGridFiller(1, 2),
        new MiniGridFiller(2, 0),
        new MiniGridFiller(2, 1),
        new MiniGridFiller(2, 2)
    };

    public static void TestHouses(int minHouseCount, int maxHouseCount)
    {
        HashSet<int> one = new();
        HashSet<int> two = new();
        
        //TODO
    }

    private static void Print(IEnumerable<int> one, IEnumerable<int> two)
    {
        var builder = new StringBuilder();

        foreach (var i in one)
        {
            builder.Append(Fillers[i] + " ");
        }

        builder.Append("--- ");

        foreach (var j in two)
        {
            builder.Append(Fillers[j] + " ");
        }

        Printer.Print(builder.ToString());
    }
}

public static class Printer
{
    private static int _count = 1;

    public static void Print(string s)
    {
        Console.WriteLine($"#{_count} : {s}");
        _count++;
    }
}

public struct IntSet
{
    private int _set;

    public void Add(int n)
    {
        _set |= 1 << n;
    }
    
    
}

public abstract class HouseFiller
{
    public int Number { get; set; }

    public abstract int HouseNumber { get; }
    
    public void Fill(int[,] grid)
    {
        foreach (var cell in AllCells())
        {
            grid[cell.Row, cell.Col] += Number;
        }
    }

    public void UnFill(int[,] grid)
    {
        foreach (var cell in AllCells())
        {
            grid[cell.Row, cell.Col] -= Number;
        }
    }

    public bool CanFill(int[,] grid)
    {
        bool result = false;
        
        foreach (var cell in AllCells())
        {
            var current = grid[cell.Row, cell.Col];
            if (current == Number) return false;
            if (current == -Number) result = true;
        }

        return result;
    }

    protected abstract IEnumerable<Cell> AllCells();
}

public class RowFiller : HouseFiller
{
    private readonly int _row;

    public RowFiller(int row)
    {
        _row = row;
    }

    public override int HouseNumber => _row;

    protected override IEnumerable<Cell> AllCells()
    {
        for (int col = 0; col < 9; col++)
        {
            yield return new Cell(_row, col);
        }
    }

    public override string ToString()
    {
        return "r" + HouseNumber;
    }
}

public class ColumnFiller : HouseFiller
{
    private readonly int _col;

    public ColumnFiller(int col)
    {
        _col = col;
    }

    public override int HouseNumber => _col;

    protected override IEnumerable<Cell> AllCells()
    {
        for (int row = 0; row < 9; row++)
        {
            yield return new Cell(row, _col);
        }
    }
    
    public override string ToString()
    {
        return "c" + HouseNumber;
    }
}

public class MiniGridFiller : HouseFiller
{
    private readonly int _miniRow;
    private readonly int _miniCol;

    public MiniGridFiller(int miniRow, int miniCol)
    {
        _miniRow = miniRow;
        _miniCol = miniCol;
    }

    public override int HouseNumber => _miniRow * 3 + _miniCol;

    protected override IEnumerable<Cell> AllCells()
    {
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                yield return new Cell(_miniRow * 3 + r, _miniCol * 3 + c);
            }
        }
    }
    
    public override string ToString()
    {
        return "m" + HouseNumber;
    }
}
