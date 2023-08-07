using System.Collections.Generic;
using Model.StrategiesUtil;

namespace Model;

public class ChangeBuffer //TODO move ReportWaiter to push function and creation of change buffer only on actual change
{
    private HashSet<CondensedPossibilityCoordinate>? _possibilityRemoved;
    private HashSet<CondensedPossibilityCoordinate>? _definitiveAdded;

    private readonly IChangeManager _m;
    private readonly IChangeReportWaiter _reportWaiter;
    private readonly IStrategy _strategy;

    public ChangeBuffer(IChangeManager changeManager, IStrategy strategy, IChangeReportWaiter reportWaiter)
    {
        _m = changeManager;
        _reportWaiter = reportWaiter;
        _strategy = strategy;
    }

    public void AddPossibilityToRemove(int possibility, int row, int col)
    {
        if (!_m.Possibilities[row, col].Peek(possibility)) return;
        _possibilityRemoved ??= new HashSet<CondensedPossibilityCoordinate>();
        _possibilityRemoved.Add(new CondensedPossibilityCoordinate(row, col, possibility));
    }

    public void AddDefinitiveToAdd(int number, int row, int col)
    {
        _definitiveAdded ??= new HashSet<CondensedPossibilityCoordinate>();
        _definitiveAdded.Add(new CondensedPossibilityCoordinate(row, col, number));
    }

    public bool Push()
    {
        List<SolverChange> changes = new();
        if (_possibilityRemoved is not null)
        {
            foreach (var possibility in _possibilityRemoved)
            {
                if (_m.RemovePossibility(possibility.Possibility, possibility.Row, possibility.Column)) changes.Add(
                    new SolverChange(SolverNumberType.Possibility,
                        possibility.Possibility, possibility.Row, possibility.Column));
            
            } 
        }

        if (_definitiveAdded is not null)
        {
            foreach (var definitive in _definitiveAdded)
            {
                if(_m.AddDefinitive(definitive.Possibility, definitive.Row, definitive.Column)) changes.Add(
                    new SolverChange(SolverNumberType.Definitive, definitive.Possibility,
                        definitive.Row, definitive.Column));
            }  
        }

        if (_m.LogsManaged && changes.Count > 0)
        {
            _reportWaiter.Process(changes, _m);
            _m.PushChangeReportLog(_reportWaiter.Process(changes, _m), _strategy);
        }

        return changes.Count > 0;
    }
}