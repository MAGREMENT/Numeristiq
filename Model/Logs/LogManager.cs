using System.Collections.Generic;
using Model.Logs;

namespace Model;

public class LogManager
{
    public List<ISolverLog> Logs { get; } = new();

    private int _lastStrategy = -1;

    private ISolverLog? _current;

    public void NumberAdded(int number, int row, int col, IStrategy strategy, int strategyCount)
    {
        if (strategyCount != _lastStrategy)
        {
            Push();
            _current = new BasicLog(strategy);
            _lastStrategy = strategyCount;
        } 
        
        _current!.DefinitiveAdded(number, row, col);
    }
    
    public void PossibilityRemoved(int possibility, int row, int col, IStrategy strategy, int strategyCount)
    {
        if (strategyCount != _lastStrategy)
        {
            Push();
            _current = new BasicLog(strategy);
            _lastStrategy = strategyCount;
        }
        
        _current!.PossibilityRemoved(possibility, row, col);
    }

    public void Push()
    {
        if (_current is not null)
        {
            Logs.Add(_current);
            _current = null;
        }
    }
}