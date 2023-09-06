using System.Collections.Generic;
using Model.Changes;
using Model.Solver;

namespace Model.Logs;

public class LogManager
{
    public List<ISolverLog> Logs { get; } = new();

    private ISolverLog? _current;
    private int _idCount = 1;

    private readonly ILogHolder _holder;
    
    public delegate void OnLogsUpdate(List<ISolverLog> logs);
    public event OnLogsUpdate? LogsUpdated;
    
    public string StartState { get; private set; }

    public LogManager(ILogHolder holder)
    {
        _holder = holder;
        StartState = holder.State;
    }

    public void NumberAdded(int number, int row, int col, IStrategy strategy)
    {
        if (_current is not BuildUpLog)
        {
            Push();
            _current = new BuildUpLog(_idCount++, strategy, _holder.State);
        } 
        
        ((BuildUpLog)_current).DefinitiveAdded(number, row, col);
    }
    
    public void PossibilityRemoved(int possibility, int row, int col, IStrategy strategy)
    {
        if (_current is not BuildUpLog)
        {
            Push();
            _current = new BuildUpLog(_idCount++, strategy, _holder.State);
        }
        
        ((BuildUpLog)_current).PossibilityRemoved(possibility, row, col);
    }

    public void PossibilityRemovedByHand(int possibility, int row, int col)
    {
        FastPush(new ByHandRemovedLog(_idCount++, possibility, row, col, _holder.State));
    }

    public void ChangePushed(ChangeReport report, IStrategy strategy)
    {
        FastPush(new ChangeReportLog(_idCount++, strategy, report, _holder.State));
    }

    public void Push()
    {
        if (_current is null) return;
        
        Logs.Add(_current);
        _current = null;
        
        LogsUpdated?.Invoke(Logs);
    }

    private void FastPush(ISolverLog log)
    {
        Push();
        _current = log;
        Push();
    }

    public void Clear()
    {
        Logs.Clear();
        _idCount = 1;
        StartState = _holder.State;

        LogsUpdated?.Invoke(Logs);
    }
}