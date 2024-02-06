using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Sudoku;
using Model.Sudoku.Solver;

namespace Model.Helpers.Logs;

public class LogManager
{
    private readonly ILogProducer _producer;
    
    public List<ISolverLog> Logs { get; } = new();
    
    public event OnLogsUpdate? LogsUpdated;
    
    private int _idCount = 1;
    private SolverState? _stateBuffer;
    private int _lastLogCount;

    public LogManager(ILogProducer producer)
    {
        _producer = producer;
    }
    
    public void Clear()
    {
        _idCount = 1;
        Logs.Clear();
        TryCallLogsUpdatedEvent();
    }

    public void StartPush()
    {
        _stateBuffer = _producer.CurrentState;
    }

    public void AddFromReport(ChangeReport report, IReadOnlyList<SolverChange> changes, ICommitMaker maker)
    {
        if (_stateBuffer == null) return;
        Logs.Add(new ChangeReportLog(_idCount++, maker, changes, report, _stateBuffer,
            _stateBuffer.Apply(changes)));
    }

    public void StopPush()
    {
        _stateBuffer = null;
        TryCallLogsUpdatedEvent();
    }

    public void AddByHand(int possibility, int row, int col, ChangeType changeType, SolverState stateBefore)
    {
        Logs.Add(new ByHandLog(_idCount++, possibility, row, col, changeType, stateBefore, _producer.CurrentState));
        TryCallLogsUpdatedEvent();
    }

    private void TryCallLogsUpdatedEvent()
    {
        if (_lastLogCount == Logs.Count) return;
        
        LogsUpdated?.Invoke();
        _lastLogCount = Logs.Count;
    }
}