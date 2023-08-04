using System.Collections.Generic;

namespace Model.Logs;

public class LogManager  //TODO work around the strategy count thingy with push()
{
    public List<ISolverLog> Logs { get; } = new();

    private int _lastStrategy = -1;

    private ISolverLog? _current;

    private int _idCount = 1;

    public void NumberAdded(int number, int row, int col, IStrategy strategy, Solver solver)
    {
        if (solver.StrategyCount != _lastStrategy || _current is not BuildUpLog)
        {
            Push();
            _current = new BuildUpLog(_idCount++, strategy, solver.State);
            _lastStrategy = solver.StrategyCount;
        } 
        
        ((BuildUpLog)_current).DefinitiveAdded(number, row, col);
    }
    
    public void PossibilityRemoved(int possibility, int row, int col, IStrategy strategy, Solver solver)
    {
        if (solver.StrategyCount != _lastStrategy || _current is not BuildUpLog)
        {
            Push();
            _current = new BuildUpLog(_idCount++, strategy, solver.State);
            _lastStrategy = solver.StrategyCount;
        }
        
        ((BuildUpLog)_current).PossibilityRemoved(possibility, row, col);
    }

    public void PossibilityRemovedByHand(int possibility, int row, int col, Solver solver)
    {
        Logs.Add(new ByHandRemovedLog(_idCount++, possibility, row, col, solver.State));
    }

    public void ChangePushed(IEnumerable<LogChange> changes, IEnumerable<LogCause> causes, string explanation,
        IStrategy strategy, string solverState)
    {
        Push();
        Logs.Add(new ChangePushedLog(_idCount++, strategy, changes, causes, explanation, solverState));
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