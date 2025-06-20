using Model.Core.BackTracking;
using Model.Core.Generators;

namespace Model.CrossSums;

public class CrossSum : IReadOnlyCrossSum, ICellsAndDigitsPuzzle, ICopyable<CrossSum>
{
    private readonly int[,] _values;
    private readonly int[] _expectedCols;
    private readonly int[] _expectedRows;
    private readonly bool[,] _chosen;

    public int RowCount => _values.GetLength(0);
    public int ColumnCount => _values.GetLength(1);

    public CrossSum(int rowCount, int colCount)
    {
        _values = new int[rowCount, colCount];
        _chosen = new bool[rowCount, colCount];
        _expectedCols = new int[colCount];
        _expectedRows = new int[rowCount];
    }

    public int ExpectedForColumn(int col) => _expectedCols[col];

    public int AddToExpectedForColumn(int col, int v) => _expectedCols[col] += v;

    public int ExpectedForRow(int row) => _expectedRows[row];
    
    public int AddToExpectedForRow(int row, int v) => _expectedRows[row] += v;

    public void Choose(int row, int col, bool chosen = true)
    {
        _chosen[row, col] = chosen;
    }

    public bool IsChosen(int row, int col)
    {
        return _chosen[row, col];
    }

    public int this[int row, int col]
    {
        get => _values[row, col];
        set => _values[row, col] = value;
    }

    public CrossSum Copy()
    {
        throw new System.NotImplementedException();
    }
}

public interface IReadOnlyCrossSum
{
    public int RowCount { get; }
    
    public int ColumnCount { get; }

    public bool IsChosen(int row, int col);
    
    public int this[int row, int col] { get; }

    public int ExpectedForColumn(int col);

    public int ExpectedForRow(int row);
}

public static class CrossSumExtensions
{
    public static (int[], int[]) GetCurrentTotals(this IReadOnlyCrossSum cs)
    {
        var colTotals = new int[cs.ColumnCount];
        var rowTotals = new int[cs.RowCount];

        for (int i = 0; i < cs.RowCount; i++)
        {
            for (int j = 0; j < cs.ColumnCount; j++)
            {
                if (cs.IsChosen(i, j))
                {
                    var v = cs[i, j];
                    colTotals[j] += v;
                    rowTotals[i] += v;
                }
            }
        }

        return (rowTotals, colTotals);
    }

    public static bool IsCorrect(this IReadOnlyCrossSum cs, int[] rowTotals, int[] colTotals)
    {
        for (int r = 0; r < cs.RowCount; r++)
        {
            if (rowTotals[r] != cs.ExpectedForRow(r)) return false;
        }

        for (int c = 0; c < cs.ColumnCount; c++)
        {
            if (colTotals[c] != cs.ExpectedForColumn(c)) return false;
        }

        return true;
    }
}