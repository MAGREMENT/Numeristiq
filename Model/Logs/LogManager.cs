using System.Collections.Generic;

namespace Model.Logs;

public class LogManager  //TODO work around the strategy count thingy with push()
{
    public List<ISolverLog> Logs { get; } = new();

    private int _lastStrategy = -1;

    private ISolverLog? _current;

    private int _idCount = 1;

    private readonly ILogHolder _holder;

    public LogManager(ILogHolder holder)
    {
        _holder = holder;
    }

    public void NumberAdded(int number, int row, int col, IStrategy strategy)
    {
        if (_holder.StrategyCount != _lastStrategy || _current is not BuildUpLog)
        {
            Push();
            _current = new BuildUpLog(_idCount++, strategy, _holder.State);
            _lastStrategy = _holder.StrategyCount;
        } 
        
        ((BuildUpLog)_current).DefinitiveAdded(number, row, col);
    }
    
    public void PossibilityRemoved(int possibility, int row, int col, IStrategy strategy)
    {
        if (_holder.StrategyCount != _lastStrategy || _current is not BuildUpLog)
        {
            Push();
            _current = new BuildUpLog(_idCount++, strategy, _holder.State);
            _lastStrategy = _holder.StrategyCount;
        }
        
        ((BuildUpLog)_current).PossibilityRemoved(possibility, row, col);
    }

    public void PossibilityRemovedByHand(int possibility, int row, int col)
    {
        Logs.Add(new ByHandRemovedLog(_idCount++, possibility, row, col, _holder.State));
    }

    public void ChangePushed(ChangeReport report, IStrategy strategy)
    {
        Push();
        Logs.Add(new ChangeReportLog(_idCount++, strategy, report, _holder.State));
    }

    public void Push()
    {
        if (_current is null) return;
        
        Logs.Add(_current);
        _current = null;
    }

    public void Clear()
    {
        Logs.Clear();
        _idCount = 1;
    }
}