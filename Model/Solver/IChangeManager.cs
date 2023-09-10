using Model.Changes;
using Model.Possibilities;

namespace Model.Solver;

public interface IChangeManager : IPossibilitiesHolder
{
    public IPossibilitiesHolder TakePossibilitiesSnapshot();
    public bool LogsManaged { get; }
    public bool AddDefinitive(int number, int row, int col);
    public bool RemovePossibility(int possibility, int row, int col);
    public void PushChangeReportLog(ChangeReport report, IStrategy strategy);
}