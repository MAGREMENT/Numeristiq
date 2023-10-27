using Model.Solver.Helpers.Changes;

namespace Model.Solver;

public interface IChangeManager : IPossibilitiesHolder
{
    public IPossibilitiesHolder TakeSnapshot();
    public bool LogsManaged { get; }
    public bool AddSolutionFromBuffer(int number, int row, int col);
    public bool RemovePossibilityFromBuffer(int possibility, int row, int col);
    public void AddCommitLog(ChangeReport report, IStrategy strategy);
}