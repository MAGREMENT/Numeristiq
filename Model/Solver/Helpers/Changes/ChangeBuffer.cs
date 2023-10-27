using System.Collections.Generic;
using Model.Solver.Positions;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Helpers.Changes;

//TODO push options + multiple commits same state + current log visual + current strategy visual
public class ChangeBuffer
{
    private readonly HashSet<CellPossibility> _possibilityRemovedBuffer = new();
    private readonly HashSet<CellPossibility> _solutionAddedBuffer = new();

    private readonly List<ChangeCommit> _commits = new();

    private readonly IChangeManager _m;

    public ChangeBuffer(IChangeManager changeManager)
    {
        _m = changeManager;
    }

    public void AddPossibilityToRemove(int possibility, int row, int col)
    {
        AddPossibilityToRemove(new CellPossibility(row, col, possibility));
    }

    public void AddPossibilityToRemove(CellPossibility cp)
    {
        if (!_m.PossibilitiesAt(cp.Row, cp.Col).Peek(cp.Possibility)) return;
        
        _possibilityRemovedBuffer.Add(cp);
    }

    public void AddSolutionToAdd(int number, int row, int col)
    {
        AddSolutionToAdd(new CellPossibility(row, col, number));
    }

    public void AddSolutionToAdd(CellPossibility cp)
    {
        if(_m.Sudoku[cp.Row, cp.Col] != 0) return;

        _solutionAddedBuffer.Add(cp);
    }

    public bool NotEmpty()
    {
        return _possibilityRemovedBuffer.Count > 0 || _solutionAddedBuffer.Count > 0;
    }

    public void Push()
    { 
        foreach (var commit in _commits)
        {
            bool yes = false;
            
            foreach (var change in commit.Changes)
            {
                if (change.ChangeType == ChangeType.Solution)
                {
                    if(_m.AddSolutionFromBuffer(change.Number, change.Row, change.Column)) yes = true;
                }
                else if (_m.RemovePossibilityFromBuffer(change.Number, change.Row, change.Column)) yes = true;
            }

            if (yes && _m.LogsManaged && commit.Report is not null) _m.AddCommitLog(commit.Report, commit.Responsible);
        }
        
        _commits.Clear();
    }

    public bool Commit(IStrategy strategy, IChangeReportBuilder builder)
    {
        if (_possibilityRemovedBuffer.Count == 0 && _solutionAddedBuffer.Count == 0) return false;

        var changes = BuffersToChangeList();
        _commits.Add(_m.LogsManaged
            ? new ChangeCommit(strategy, changes, builder.Build(changes, _m.TakeSnapshot()))
            : new ChangeCommit(strategy, changes));

        return true;
    }
    
    private List<SolverChange> BuffersToChangeList()
    {
        List<SolverChange> changes = new();
        
        foreach (var solution in _solutionAddedBuffer)
        {
            changes.Add(new SolverChange(ChangeType.Solution, solution.Possibility, 
                solution.Row, solution.Col));
        }
        
        foreach (var possibility in _possibilityRemovedBuffer)
        {
            changes.Add(new SolverChange(ChangeType.Possibility, 
                possibility.Possibility, possibility.Row, possibility.Col));
        }
        
        _possibilityRemovedBuffer.Clear();
        _solutionAddedBuffer.Clear();

        return changes;
    }
}

public class ChangeCommit
{
    public IStrategy Responsible { get; }
    public List<SolverChange> Changes { get; }
    public ChangeReport? Report { get; }

    public ChangeCommit(IStrategy responsible, List<SolverChange> changes, ChangeReport report)
    {
        Responsible = responsible;
        Changes = changes;
        Report = report;
    }

    public ChangeCommit(IStrategy responsible, List<SolverChange> changes)
    {
        Responsible = responsible;
        Changes = changes;
        Report = null;
    }
}