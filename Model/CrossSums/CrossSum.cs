using System.Text;
using Model.Core.BackTracking;
using Model.Core.Generators;
using Model.Utility;

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

    private CrossSum(CrossSum cs)
    {
        _values = new int[cs.RowCount, cs.ColumnCount];
        _chosen = new bool[cs.RowCount, cs.ColumnCount];
        _expectedCols = new int[cs.ColumnCount];
        _expectedRows = new int[cs.RowCount];
        
        cs._values.CopyTo(_values, 0);
        cs._chosen.CopyTo(_chosen, 0);
        cs._expectedCols.CopyTo(_expectedCols, 0);
        cs._expectedRows.CopyTo(_expectedRows, 0);
    }

    public int GetExpectedForColumn(int col) => _expectedCols[col];

    public int AddToExpectedForColumn(int col, int v) => _expectedCols[col] += v;

    public int GetExpectedForRow(int row) => _expectedRows[row];
    
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

    public CrossSum Copy() => new CrossSum(this);

    public override string ToString()
    {
        var builder = new StringBuilder("xxx|");

        for (int c = 0; c < ColumnCount; c++)
        {
            builder.Append(_expectedCols[c].ToString().FillEvenlyWith(' ', 3));
            builder.Append('|');
        }

        var total = ColumnCount * 3 + ColumnCount + 4;
        builder.Append('\n');
        builder.Append('-'.Repeat(total));
        builder.Append('\n');
        for (int r = 0; r < RowCount; r++)
        {
            builder.Append(_expectedRows[r].ToString().FillEvenlyWith(' ', 3));
            builder.Append('|');

            for (int c = 0; c < ColumnCount; c++)
            {
                builder.Append(IsChosen(r, c) ? '(' : ' ');
                builder.Append(this[r, c]);
                builder.Append(IsChosen(r, c) ? ')' : ' ');
                builder.Append('|');
            }
            
            builder.Append('\n');
            builder.Append('-'.Repeat(total));
            builder.Append('\n');
        }
        
        return builder.ToString();
    }
}

public interface IReadOnlyCrossSum
{
    public int RowCount { get; }
    
    public int ColumnCount { get; }

    public bool IsChosen(int row, int col);
    
    public int this[int row, int col] { get; }

    public int GetExpectedForColumn(int col);

    public int GetExpectedForRow(int row);
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
            if (rowTotals[r] != cs.GetExpectedForRow(r)) return false;
        }

        for (int c = 0; c < cs.ColumnCount; c++)
        {
            if (colTotals[c] != cs.GetExpectedForColumn(c)) return false;
        }

        return true;
    }
}