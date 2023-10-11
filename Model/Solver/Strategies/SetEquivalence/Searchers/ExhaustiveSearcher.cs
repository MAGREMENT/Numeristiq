using System.Collections.Generic;

namespace Model.Solver.Strategies.SetEquivalence.Searchers;

public class ExhaustiveSearcher : ISetEquivalenceSearcher
{
    private readonly int _maxOrderDifference;
    private readonly int _maxHouseCount;

    private readonly IHouseFiller[] _fillers =
    {
        new RowFiller(1),
        new RowFiller(2),
        new RowFiller(3),
        new RowFiller(4),
        new RowFiller(5),
        new RowFiller(6),
        new RowFiller(7),
        new RowFiller(8),
        new RowFiller(9),
        new ColumnFiller(1),
        new ColumnFiller(2),
        new ColumnFiller(3),
        new ColumnFiller(4),
        new ColumnFiller(5),
        new ColumnFiller(6),
        new ColumnFiller(7),
        new ColumnFiller(8),
        new ColumnFiller(9),
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

    public ExhaustiveSearcher(int maxOrderDifference, int maxHouseCount)
    {
        _maxOrderDifference = maxOrderDifference;
        _maxHouseCount = maxHouseCount;
    }
    
    public IEnumerable<SetEquivalence> Search(IStrategyManager strategyManager)
    {
        //Too heavy to do
        yield break;
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