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
            _current = new NumberAddedLog(number, row, col, strategy);
            _lastStrategy = strategyCount;
        }
        else _current!.Another(number, row, col);
    }
    
    public void PossibilityRemoved(int possibility, int row, int col, IStrategy strategy, int strategyCount)
    {
        if (strategyCount != _lastStrategy)
        {
            Push();
            _current = new PossibilityRemovedLog(possibility, row, col, strategy);
            _lastStrategy = strategyCount;
        }
        else _current!.Another(possibility, row, col);
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