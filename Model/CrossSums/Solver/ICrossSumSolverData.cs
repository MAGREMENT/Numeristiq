namespace Model.CrossSums.Solver;

public interface ICrossSumSolverData
{
    public IReadOnlyCrossSum CrossSum { get; }

    public int GetTotalForRow(int row);

    public int GetTotalForColumn(int col);

    public bool IsAvailable(int row, int col);
}