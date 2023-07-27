using System.Collections.Generic;

namespace Model.Logs;

public class LogManager
{
    public List<ISolverLog> Logs { get; } = new();

    private int _lastStrategy = -1;

    private ISolverLog? _current;

    public void NumberAdded(int number, int row, int col, IStrategy strategy, Solver solver)
    {
        if (solver.StrategyCount != _lastStrategy)
        {
            Push();
            _current = new BasicLog(strategy, solver.State);
            _lastStrategy = solver.StrategyCount;
        } 
        
        _current!.DefinitiveAdded(number, row, col);
    }
    
    public void PossibilityRemoved(int possibility, int row, int col, IStrategy strategy, Solver solver)
    {
        if (solver.StrategyCount != _lastStrategy)
        {
            Push();
            _current = new BasicLog(strategy, solver.State);
            _lastStrategy = solver.StrategyCount;
        }
        
        _current!.PossibilityRemoved(possibility, row, col);
    }

    public void PossibilityRemovedByHand(int possibility, int row, int col, Solver solver)
    {
        Logs.Add(new ByHandRemovedLog(possibility, row, col, solver.State));
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