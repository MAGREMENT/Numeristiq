using System.Collections.Generic;
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

    public void ProposePossibilityRemoval(int possibility, int row, int col)
    {
        ProposePossibilityRemoval(new CellPossibility(row, col, possibility));
    }

    public void ProposePossibilityRemoval(CellPossibility cp)
    {
        if (!_m.PossibilitiesAt(cp.Row, cp.Col).Peek(cp.Possibility)) return;
        
        _possibilityRemovedBuffer.Add(cp);
    }

    public void ProposeSolutionAddition(int number, int row, int col)
    {
        ProposeSolutionAddition(new CellPossibility(row, col, number));
    }

    public void ProposeSolutionAddition(CellPossibility cp)
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
        if(_m.LogManager.IsEnabled) PushWithLogsManaged();
        else PushWithoutLogsManaged();
    }

    private void PushWithoutLogsManaged()
    {
        foreach (var commit in _commits)
        {
            foreach (var change in commit.Changes)
            {
                _m.ExecuteChange(change);
            }
        }
        
        _commits.Clear();
    }

    private void PushWithLogsManaged()
    {
        _m.LogManager.StartPush();
        var snapshot = _m.TakeSnapshot();
        
        foreach (var commit in _commits)
        {
            List<SolverChange> impactfulChanges = new();
            
            foreach (var change in commit.Changes)
            {
                if (_m.ExecuteChange(change)) impactfulChanges.Add(change);
            }

            if (commit.Builder is null || impactfulChanges.Count == 0) continue;
            _m.LogManager.AddFromReport(commit.Builder.Build(impactfulChanges, snapshot), impactfulChanges, commit.Responsible);
        }
        
        _commits.Clear();
        _m.LogManager.StopPush();
    }

    public bool Commit(IStrategy strategy, IChangeReportBuilder builder)
    {
        if (_possibilityRemovedBuffer.Count == 0 && _solutionAddedBuffer.Count == 0) return false;

        var changes = BuffersToChangeList();
        _commits.Add(_m.LogManager.IsEnabled
            ? new ChangeCommit(strategy, changes, builder)
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
    public IChangeReportBuilder? Builder { get; }

    public ChangeCommit(IStrategy responsible, List<SolverChange> changes, IChangeReportBuilder builder)
    {
        Responsible = responsible;
        Changes = changes;
        Builder = builder;
    }

    public ChangeCommit(IStrategy responsible, List<SolverChange> changes)
    {
        Responsible = responsible;
        Changes = changes;
        Builder = null;
    }
}