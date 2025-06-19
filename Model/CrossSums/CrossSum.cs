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