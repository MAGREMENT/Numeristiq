using System.Collections.Generic;
using Model.StrategiesUtil;

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

    public void ChangePushed(IEnumerable<LogChange> changes, IEnumerable<LogCause> causes, string explanation,
        IStrategy strategy)
    {
        Push();
        Logs.Add(new ChangePushedLog(_idCount++, strategy, changes, causes, explanation, _holder.State));
    }

    public void Push()
    {
        if (_current is null) return;
        
        Logs.Add(_current);
        _current = null;
    }
}