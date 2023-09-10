using System.Collections.Generic;
using Model.Solver;
using Model.StrategiesUtil;

namespace Model.Changes;

public class ChangeBuffer
{
    private readonly HashSet<CellPossibility> _possibilityRemoved = new();
    private readonly HashSet<CellPossibility> _definitiveAdded = new();

    private readonly IChangeManager _m;

    public ChangeBuffer(IChangeManager changeManager)
    {
        _m = changeManager;
    }

    public void AddPossibilityToRemove(int possibility, int row, int col)
    {
        AddPossibilityToRemove(new CellPossibility(row, col, possibility));
    }

    public void AddPossibilityToRemove(CellPossibility coord)
    {
        if (!_m.Possibilities[coord.Row, coord.Col].Peek(coord.Possibility)) return;
        _possibilityRemoved.Add(coord);
    }

    public void AddDefinitiveToAdd(int number, int row, int col)
    {
        AddDefinitiveToAdd(new CellPossibility(row, col, number));
    }

    public void AddDefinitiveToAdd(CellPossibility coord)
    {
        _definitiveAdded.Add(coord);
    }

    public bool NotEmpty()
    {
        return _possibilityRemoved.Count > 0 || _definitiveAdded.Count > 0;
    }

    public bool Push(IStrategy strategy, IChangeReportBuilder builder)
    {
        if (_possibilityRemoved.Count == 0 && _definitiveAdded.Count == 0) return false;
        
        List<SolverChange> changes = new();
        foreach (var possibility in _possibilityRemoved)
        {
            if (_m.RemovePossibility(possibility.Possibility, possibility.Row, possibility.Col)) changes.Add(
                new SolverChange(SolverNumberType.Possibility,
                    possibility.Possibility, possibility.Row, possibility.Col));
            
        }
        
        foreach (var definitive in _definitiveAdded)
        {
            if(_m.AddDefinitive(definitive.Possibility, definitive.Row, definitive.Col)) changes.Add(
                new SolverChange(SolverNumberType.Definitive, definitive.Possibility,
                    definitive.Row, definitive.Col));
        }

        if (changes.Count > 0) if (_m.LogsManaged) _m.PushChangeReportLog(builder.Build(changes, _m), strategy);

        _possibilityRemoved.Clear();
        _definitiveAdded.Clear();
        return changes.Count > 0;
    }
}