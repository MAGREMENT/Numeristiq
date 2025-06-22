using Model.Core;
using Model.Core.Changes;

namespace Model.CrossSums.Solver;

public interface ICrossSumSolverData
{
    public IReadOnlyCrossSum CrossSum { get; }

    public int GetTotalForRow(int row);

    public int GetTotalForColumn(int col);

    public int GetAvailableForRow(int row);

    public int GetAvailableForColumn(int col);

    public bool IsAvailable(int row, int col);
    
    public DichotomousChangeBuffer<IDichotomousSolvingState, ICrossSumHighlighter> ChangeBuffer { get; }
}

public static class CrossSumSolverDataExtensions
{
    public static int GetRemainingForRow(this ICrossSumSolverData data, int row)
        => data.CrossSum.GetExpectedForRow(row) - data.GetTotalForRow(row);
    
    public static int GetRemainingForColumn(this ICrossSumSolverData data, int col)
        => data.CrossSum.GetExpectedForColumn(col) - data.GetTotalForColumn(col);
}