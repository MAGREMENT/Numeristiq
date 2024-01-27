using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.SudokuSolving.Solver.StrategiesUtility;

namespace Model.SudokuSolving.Solver.Helpers.Changes;

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
        if (!_m.PossibilitiesAt(cp.Row, cp.Column).Peek(cp.Possibility)) return;
        
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
        if(_m.Sudoku[cp.Row, cp.Column] != 0) return;

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

    public void PushCommit(BuiltChangeCommit commit)
    {
        _m.LogManager.StartPush();

        foreach (var change in commit.Changes)
        {
            _m.ExecuteChange(change);
        }
        _m.LogManager.AddFromReport(commit.Report, commit.Changes, commit.Responsible);
        
        _m.LogManager.StopPush();
    }
    
    private SolverChange[] BuffersToChangeList()
    {
        var count = 0;
        var changes = new SolverChange[_solutionAddedBuffer.Count + _possibilityRemovedBuffer.Count];
        
        foreach (var solution in _solutionAddedBuffer)
        {
            changes[count++] = new SolverChange(ChangeType.Solution, solution.Possibility,
                solution.Row, solution.Column);
        }
        
        foreach (var possibility in _possibilityRemovedBuffer)
        {
            changes[count++] = new SolverChange(ChangeType.Possibility, 
                possibility.Possibility, possibility.Row, possibility.Column);
        }
        
        _possibilityRemovedBuffer.Clear();
        _solutionAddedBuffer.Clear();

        return changes;
    }

    public BuiltChangeCommit[] DumpBuiltCommits()
    {
        var snapshot = _m.TakeSnapshot();
        List<BuiltChangeCommit> result = new(_commits.Count);
        
        foreach (var c in _commits)
        {
            if (c.Builder is null) continue;
            result.Add(new BuiltChangeCommit(c.Responsible, c.Changes, c.Builder.Build(c.Changes, snapshot)));
        }
        
        _commits.Clear();
        return result.ToArray();
    }
}

public class ChangeCommit
{
    public IStrategy Responsible { get; }
    public SolverChange[] Changes { get; }
    public IChangeReportBuilder? Builder { get; }

    public ChangeCommit(IStrategy responsible, SolverChange[] changes, IChangeReportBuilder builder)
    {
        Responsible = responsible;
        Changes = changes;
        Builder = builder;
    }

    public ChangeCommit(IStrategy responsible, SolverChange[] changes)
    {
        Responsible = responsible;
        Changes = changes;
        Builder = null;
    }
}

public class BuiltChangeCommit
{
    public BuiltChangeCommit(IStrategy responsible, SolverChange[] changes, ChangeReport report)
    {
        Responsible = responsible;
        Changes = changes;
        Report = report;
    }

    public IStrategy Responsible { get; }
    public SolverChange[] Changes { get; }
    public ChangeReport Report { get; }
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