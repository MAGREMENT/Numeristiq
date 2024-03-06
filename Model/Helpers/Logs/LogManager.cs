using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Sudoku;

namespace Model.Helpers.Logs;

public class LogManager
{
    public List<ISolverLog> Logs { get; } = new();
    
    public event OnLogsUpdate? LogsUpdated;
    
    private int _idCount = 1;
    private int _lastLogCount;
    
    public void Clear()
    {
        _idCount = 1;
        Logs.Clear();
        TryCallLogsUpdatedEvent();
    }

    public void AddFromReport(ChangeReport report, IReadOnlyList<SolverProgress> changes, ICommitMaker maker, IUpdatableSolvingState stateBefore)
    {
        Logs.Add(new ChangeReportLog(_idCount++, maker, changes, report, stateBefore));
        TryCallLogsUpdatedEvent();
    }

    public void AddByHand(int possibility, int row, int col, ProgressType progressType, IUpdatableSolvingState stateBefore)
    {
        Logs.Add(new ByHandLog(_idCount++, possibility, row, col, progressType, stateBefore));
        TryCallLogsUpdatedEvent();
    }

    private void TryCallLogsUpdatedEvent()
    {
        if (_lastLogCount == Logs.Count) return;
        
        LogsUpdated?.Invoke();
        _lastLogCount = Logs.Count;
    }
}