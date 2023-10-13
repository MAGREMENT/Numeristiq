using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Solver.Strategies.SetEquivalence.Searchers;

public static class SearcherUtils
{
    public static readonly IHouseFiller[] Fillers =
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
    
    public static void AllValidCombinations(int maxOrderDifference, int maxHouseCount)
    {
        One(maxOrderDifference, maxHouseCount, new List<int>(), 0);
    }

    private static void One(int maxOrderDifference, int maxHouseCount, List<int> one, int start)
    {
        if (one.Count >= maxHouseCount) return;
        for (int i = start; i < Fillers.Length; i++)
        {
            one.Add(i);

            Two(maxOrderDifference, maxHouseCount, one, new List<int>(), 0);
            One(maxOrderDifference, maxHouseCount, one, i + 1);

            one.RemoveAt(one.Count - 1);
        }
    }

    private static void Two(int maxOrderDifference, int maxHouseCount, List<int> one, List<int> two, int start)
    {
        if (two.Count >= maxHouseCount || two.Count - one.Count >= maxOrderDifference) return;
        for (int i = start; i < Fillers.Length; i++)
        {
            if (one.Contains(i)) continue;
            two.Add(i);

            if(Math.Abs(two.Count - one.Count) <= maxOrderDifference)Test(one, two);
            Two(maxOrderDifference, maxHouseCount, one, two, i + 1);

            two.RemoveAt(two.Count - 1);
        }
    }

    public static void Test(List<int> one, List<int> two)
    {
        int[,] grid = new int[9, 9];

        foreach (var i in one)
        {
            var filler = Fillers[i];
            filler.Number = 1;
            filler.Fill(grid);
        }
        
        foreach (var i in two)
        {
            var filler = Fillers[i];
            filler.Number = -1;
            filler.Fill(grid);
        }

        foreach (var value in grid)
        {
            if (value is > 1 or < -1) return;
        }

        for (int row = 0; row < 9; row++)
        {
            var value = grid[row, 0];
            if (value == 0) continue;

            bool notGood = true;

            for (int col = 1; col < 9; col++)
            {
                if (grid[row, col] == 0 || grid[row, col] != value)
                {
                    notGood = false;
                    break;
                }
            }

            if (notGood) return;
        }

        for (int col = 0; col < 9; col++)
        {
            var value = grid[0, col];
            if (value == 0) continue;

            bool notGood = true;

            for (int row = 1; row < 9; row++)
            {
                if (grid[row, col] == 0 || grid[row, col] != value)
                {
                    notGood = false;
                    break;
                }
            }

            if (notGood) return;
        }

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                var value = grid[miniRow * 3, miniCol * 3];
                if (value == 0) continue;

                bool notGood = true;

                for (int gridRow = 0; gridRow < 3; gridRow++)
                {
                    for (int gridCol = 0; gridCol < 3; gridCol++)
                    {
                        var row = miniRow * 3 + gridRow;
                        var col = miniCol * 3 + gridCol;
                        
                        if (grid[row, col] == 0 || grid[row, col] != value)
                        {
                            notGood = false;
                            break;
                        }
                    }
                }
                
                if (notGood) return;
            }
        }
        
        //IsOk
        Print(one, two);
    }

    private static void Print(List<int> one, List<int> two)
    {
        var builder = new StringBuilder();

        foreach (var i in one)
        {
            builder.Append(i + " ");
        }

        builder.Append("--- ");

        foreach (var j in two)
        {
            builder.Append(j + " ");
        }

        Printer.Print(builder.ToString());
    }
}

public static class Printer
{
    private static int count = 1;

    public static void Print(string s)
    {
        Console.WriteLine($"#{count} : {s}");
        count++;
    }
}

public interface IHouseFiller
{
    int Number { get; set; }
    void Fill(int[,] grid);
    void UnFill(int[,] grid);
}

public class RowFiller : IHouseFiller
{
    private readonly int _row;
    public RowFiller(int row)
    {
        _row = row;
    }

    public int Number { get; set; }
    public void Fill(int[,] grid)
    {
        for (int col = 0; col < 9; col++)
        {
            grid[_row, col] += Number;
        }
    }

    public void UnFill(int[,] grid)
    {
        for (int col = 0; col < 9; col++)
        {
            grid[_row, col] -= Number;
        }
    }
}

public class ColumnFiller : IHouseFiller
{
    private readonly int _col;

    public ColumnFiller(int col)
    {
        _col = col;
    }

    public int Number { get; set; }
    public void Fill(int[,] grid)
    {
        for (int row = 0; row < 9; row++)
        {
            grid[row, _col] += Number;
        }
    }

    public void UnFill(int[,] grid)
    {
        for (int row = 0; row < 9; row++)
        {
            grid[row, _col] -= Number;
        }
    }
}

public class MiniGridFiller : IHouseFiller
{
    private readonly int _miniRow;
    private readonly int _miniCol;

    public MiniGridFiller(int miniRow, int miniCol)
    {
        _miniRow = miniRow;
        _miniCol = miniCol;
    }

    public int Number { get; set; }
    public void Fill(int[,] grid)
    {
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                grid[_miniRow * 3 + gridRow, _miniCol * 3 + gridCol] += Number;
            }
        }
    }

    public void UnFill(int[,] grid)
    {
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                grid[_miniRow * 3 + gridRow, _miniCol * 3 + gridCol] -= Number;
            }
        }
    }
}