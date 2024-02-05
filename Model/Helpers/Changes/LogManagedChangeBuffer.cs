using System.Collections.Generic;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Helpers.Changes;

public class LogManagedChangeBuffer : IChangeBuffer
{
    private readonly HashSet<CellPossibility> _possibilityRemovedBuffer = new();
    private readonly HashSet<CellPossibility> _solutionAddedBuffer = new();
        
    private readonly List<ChangeCommit> _commits = new();

    private readonly ILogManagedChangeProducer _producer;

    private readonly IPushHandler[] _pushHandlers =
    {
        new ReturnPushHandler(), new WaitForAllPushHandler(), new ChooseBestPushHandler()
    };

    public LogManagedChangeBuffer(ILogManagedChangeProducer changeProducer)
    {
        _producer = changeProducer;
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
        if (_producer.CanRemovePossibility(cp)) _possibilityRemovedBuffer.Add(cp);
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
        if(_producer.CanAddSolution(cp)) _solutionAddedBuffer.Add(cp);
    }

    public bool NotEmpty()
    {
        return _possibilityRemovedBuffer.Count > 0 || _solutionAddedBuffer.Count > 0;
    }
    
    public bool Commit(IChangeReportBuilder builder)
    {
        if (_possibilityRemovedBuffer.Count == 0 && _solutionAddedBuffer.Count == 0) return false;

        var changes = BuffersToChangeList();
        _commits.Add(_producer.LogManager.IsEnabled
            ? new ChangeCommit(changes, builder)
            : new ChangeCommit(changes));

        return true;
    }

    public void Push(IStrategy pusher)
    {
        if (_commits.Count == 0) return;

        var handler = _pushHandlers[(int)pusher.OnCommitBehavior];

        if (_producer.LogManager.IsEnabled) handler.PushWithLogsManaged(pusher, _commits, _producer);
        else handler.PushWithoutLogsManaged(pusher, _commits, _producer);
        
        _commits.Clear();
    }

    public void PushCommit(BuiltChangeCommit commit) //TODO FIX
    {
        /*_m.LogManager.StartPush();

        foreach (var change in commit.Changes)
        {
            _m.ExecuteChange(change);
        }
        _m.LogManager.AddFromReport(commit.Report, commit.Changes, commit.Responsible);
        
        _m.LogManager.StopPush();*/
    }
    
    public BuiltChangeCommit[] DumpCommits()  //TODO FIX
    {
        var snapshot = _producer.TakeSnapshot();
        List<BuiltChangeCommit> result = new(_commits.Count);
        
        foreach (var c in _commits)
        {
            if (c.Builder is null) continue;
            result.Add(new BuiltChangeCommit(c.Changes, c.Builder.Build(c.Changes, snapshot)));
        }
        
        _commits.Clear();
        return result.ToArray();
    }
    
    private SolverChange[] BuffersToChangeList()
    {
        var count = 0;
        var changes = new SolverChange[_solutionAddedBuffer.Count + _possibilityRemovedBuffer.Count];
        
        foreach (var solution in _solutionAddedBuffer)
        {
            changes[count++] = new SolverChange(ChangeType.Solution, solution);
        }
        
        foreach (var possibility in _possibilityRemovedBuffer)
        {
            changes[count++] = new SolverChange(ChangeType.Possibility, possibility);
        }
        
        _possibilityRemovedBuffer.Clear();
        _solutionAddedBuffer.Clear();

        return changes;
    }
}

public interface IPushHandler
{
    void PushWithLogsManaged(IStrategy pusher, List<ChangeCommit> commits, ILogManagedChangeProducer producer);
    void PushWithoutLogsManaged(IStrategy pusher, List<ChangeCommit> commits, IChangeProducer producer);
}

public class ReturnPushHandler : IPushHandler
{
    public void PushWithLogsManaged(IStrategy pusher, List<ChangeCommit> commits, ILogManagedChangeProducer producer)
    {
        producer.LogManager.StartPush();
        var snapshot = producer.TakeSnapshot();

        var commit = commits[0];
        
        foreach (var change in commit.Changes)
        { 
            producer.ExecuteChange(change);
        }

        if (commit.Builder is not null) producer.LogManager.AddFromReport(commit.Builder.Build(commit.Changes, snapshot),
            commit.Changes, pusher);
        
        producer.LogManager.StopPush();
    }

    public void PushWithoutLogsManaged(IStrategy pusher, List<ChangeCommit> commits, IChangeProducer producer)
    {
        foreach (var change in commits[0].Changes)
        {
            producer.ExecuteChange(change);
        }
    }
}

public class WaitForAllPushHandler : IPushHandler
{
    public void PushWithLogsManaged(IStrategy pusher, List<ChangeCommit> commits, ILogManagedChangeProducer producer)
    {
        producer.LogManager.StartPush();
        var snapshot = producer.TakeSnapshot();
        
        foreach (var commit in commits)
        {
            List<SolverChange> impactfulChanges = new();
            
            foreach (var change in commit.Changes)
            {
                if (producer.ExecuteChange(change)) impactfulChanges.Add(change);
            }

            if (commit.Builder is null || impactfulChanges.Count == 0) continue;
            producer.LogManager.AddFromReport(commit.Builder.Build(impactfulChanges, snapshot), impactfulChanges, pusher);
        }
        
        producer.LogManager.StopPush();
    }

    public void PushWithoutLogsManaged(IStrategy pusher, List<ChangeCommit> commits, IChangeProducer producer)
    {
        foreach (var commit in commits)
        {
            foreach (var change in commit.Changes)
            {
                producer.ExecuteChange(change);
            }
        }
    }
}

public class ChooseBestPushHandler : IPushHandler
{
    private readonly ICustomCommitComparer _default = new DefaultCommitComparer();
    
    public void PushWithLogsManaged(IStrategy pusher, List<ChangeCommit> commits, ILogManagedChangeProducer producer)
    {
        producer.LogManager.StartPush();
        var snapshot = producer.TakeSnapshot();

        var commit = GetBest(pusher, commits);
        
        foreach (var change in commit.Changes)
        { 
            producer.ExecuteChange(change);
        }

        if (commit.Builder is not null) producer.LogManager.AddFromReport(commit.Builder.Build(commit.Changes, snapshot),
            commit.Changes, pusher);
        
        producer.LogManager.StopPush();
    }

    public void PushWithoutLogsManaged(IStrategy pusher, List<ChangeCommit> commits, IChangeProducer producer)
    {
        foreach (var change in GetBest(pusher, commits).Changes)
        {
            producer.ExecuteChange(change);
        }
    }

    private ChangeCommit GetBest(IStrategy pusher, List<ChangeCommit> commits)
    {
        var best = commits[0];
        var comparer = pusher as ICustomCommitComparer ?? _default;

        for (int i = 1; i < commits.Count; i++)
        {
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