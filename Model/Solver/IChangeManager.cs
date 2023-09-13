using Model.Changes;
using Model.Possibilities;

namespace Model.Solver;

public interface IChangeManager : IPossibilitiesHolder
{
    public IPossibilitiesHolder TakePossibilitiesSnapshot();
    public bool LogsManaged { get; }
    public bool AddSolutionFromBuffer(int number, int row, int col);
    public bool RemovePossibilityFromBuffer(int possibility, int row, int col);
    public void PublishChangeReport(ChangeReport report, IStrategy strategy);
}