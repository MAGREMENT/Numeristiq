using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.Helpers.Changes;

public class ChangeBuffer
{
    private readonly HashSet<CellPossibility> _possibilityRemovedBuffer = new();
    private readonly HashSet<CellPossibility> _solutionAddedBuffer = new();
        
    private readonly List<ChangeCommit> _commits = new();

    private readonly IChangeManager _m;

    private readonly IPushHandler[] _pushHandlers =
    {
        new ReturnPushHandler(), new WaitForAllPushHandler(), new ChooseBestPushHandler()
    };

    public ChangeBuffer(IChangeManager changeManager)
    {
        _m = changeManager;
    }

    public void ProposePossibilityRemoval(int possibility, Cell cell)
    {
        ProposePossibilityRemoval(new CellPossibility(cell, possibility));
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
    
    public void ProposeSolutionAddition(int number, Cell cell)
    {
        ProposeSolutionAddition(new CellPossibility(cell, number));
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

    public void Push(IStrategy pusher)
    {
        if (_commits.Count == 0) return;

        var handler = _pushHandlers[(int)pusher.OnCommitBehavior];

        if (_m.LogManager.IsEnabled) handler.PushWithLogsManaged(_commits, _m);
        else handler.PushWithoutLogsManaged(_commits, _m);
        
        _commits.Clear();
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

public interface IPushHandler
{
    void PushWithLogsManaged(List<ChangeCommit> commits, IChangeManager manager);
    void PushWithoutLogsManaged(List<ChangeCommit> commits, IChangeManager manager);
}

public class ReturnPushHandler : IPushHandler
{
    public void PushWithLogsManaged(List<ChangeCommit> commits, IChangeManager manager)
    {
        manager.LogManager.StartPush();
        var snapshot = manager.TakeSnapshot();

        var commit = commits[0];
        
        foreach (var change in commit.Changes)
        { 
            manager.ExecuteChange(change);
        }

        if (commit.Builder is not null) manager.LogManager.AddFromReport(commit.Builder.Build(commit.Changes, snapshot),
            commit.Changes, commit.Responsible);
        
        manager.LogManager.StopPush();
    }

    public void PushWithoutLogsManaged(List<ChangeCommit> commits, IChangeManager manager)
    {
        foreach (var change in commits[0].Changes)
        {
            manager.ExecuteChange(change);
        }
    }
}

public class WaitForAllPushHandler : IPushHandler
{
    public void PushWithLogsManaged(List<ChangeCommit> commits, IChangeManager manager)
    {
        manager.LogManager.StartPush();
        var snapshot = manager.TakeSnapshot();
        
        foreach (var commit in commits)
        {
            List<SolverChange> impactfulChanges = new();
            
            foreach (var change in commit.Changes)
            {
                if (manager.ExecuteChange(change)) impactfulChanges.Add(change);
            }

            if (commit.Builder is null || impactfulChanges.Count == 0) continue;
            manager.LogManager.AddFromReport(commit.Builder.Build(impactfulChanges, snapshot), impactfulChanges, commit.Responsible);
        }
        
        manager.LogManager.StopPush();
    }

    public void PushWithoutLogsManaged(List<ChangeCommit> commits, IChangeManager manager)
    {
        foreach (var commit in commits)
        {
            foreach (var change in commit.Changes)
            {
                manager.ExecuteChange(change);
            }
        }
    }
}

public class ChooseBestPushHandler : IPushHandler
{
    private readonly ICustomCommitComparer _default = new DefaultCommitComparer();
    
    public void PushWithLogsManaged(List<ChangeCommit> commits, IChangeManager manager)
    {
        manager.LogManager.StartPush();
        var snapshot = manager.TakeSnapshot();

        var commit = GetBest(commits);
        
        foreach (var change in commit.Changes)
        { 
            manager.ExecuteChange(change);
        }

        if (commit.Builder is not null) manager.LogManager.AddFromReport(commit.Builder.Build(commit.Changes, snapshot),
            commit.Changes, commit.Responsible);
        
        manager.LogManager.StopPush();
    }

    public void PushWithoutLogsManaged(List<ChangeCommit> commits, IChangeManager manager)
    {
        foreach (var change in GetBest(commits).Changes)
        {
            manager.ExecuteChange(change);
        }
    }

    private ChangeCommit GetBest(List<ChangeCommit> commits)
    {
        var best = commits[0];

        for (int i = 1; i < commits.Count; i++)
        {
            var comparer = best.Responsible as ICustomCommitComparer ?? _default;
            if (comparer.Compare(best, commits[i]) < 0) best = commits[i];
        }

        return best;
    }
}

public interface ICustomCommitComparer
{
    /// <summary>
    /// Compare two commits
    /// more than 0 if first is better
    /// == 0 if same
    /// less than 0 if second is better
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public int Compare(ChangeCommit first, ChangeCommit second);
}

public class DefaultCommitComparer : ICustomCommitComparer
{
    private const int SolutionAddedValue = 3;
    private const int PossibilityRemovedValue = 1;
    
    public int Compare(ChangeCommit first, ChangeCommit second)
    {
        int firstScore = 0;
        int secondScore = 0;

        foreach (var change in first.Changes)
        {
            firstScore += change.ChangeType == ChangeType.Solution ? SolutionAddedValue : PossibilityRemovedValue;
        }

        foreach (var change in second.Changes)
        {
            secondScore += change.ChangeType == ChangeType.Solution ? SolutionAddedValue : PossibilityRemovedValue;
        }

        return firstScore - secondScore;
    }
}