using System.Collections.Generic;
using Global.Enums;
using Model.Solver.Helpers.Changes;

namespace Model.Solver.Helpers.Logs;

public class LogManager
{
    private readonly ILogHolder _holder;

    public bool IsEnabled { get; set; }
    public List<ISolverLog> Logs { get; } = new();
    
    public event OnLogsUpdate? LogsUpdated;
    
    private int _idCount = 1;
    private SolverState? _stateBuffer;
    private int _lastLogCount;

    public LogManager(ILogHolder holder)
    {
        _holder = holder;
    }
    
    public void Clear()
    {
        if (!IsEnabled) return;

        _idCount = 1;
        Logs.Clear();
        TryCallLogsUpdatedEvent();
    }

    public void StartPush()
    {
        if (!IsEnabled) return;

        _stateBuffer = _holder.CurrentState;
    }

    public void AddFromReport(ChangeReport report, List<SolverChange> changes, IStrategy strategy)
    {
        if (!IsEnabled || _stateBuffer == null) return;
        Logs.Add(new ChangeReportLog(_idCount++, strategy, report, _stateBuffer, _stateBuffer.Apply(changes)));
    }

    public void StopPush()
    {
        if (!IsEnabled) return;

        _stateBuffer = null;
        TryCallLogsUpdatedEvent();
    }

    public void AddByHand(int possibility, int row, int col, ChangeType changeType)
    {
        if (!IsEnabled) return;
        
        //TODO correct this
        Logs.Add(new ByHandLog(_idCount++, possibility, row, col, changeType, _holder.CurrentState, _holder.CurrentState));
        TryCallLogsUpdatedEvent();
    }

    private void TryCallLogsUpdatedEvent()
    {
        if (_lastLogCount == Logs.Count) return;
        
        LogsUpdated?.Invoke();
        _lastLogCount = Logs.Count;
    }
}