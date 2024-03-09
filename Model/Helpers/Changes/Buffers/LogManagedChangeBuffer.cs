using System.Collections.Generic;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Helpers.Changes.Buffers;

public class LogManagedChangeBuffer : IChangeBuffer
{
    private readonly HashSet<CellPossibility> _possibilityRemovedBuffer = new();
    private readonly HashSet<CellPossibility> _solutionAddedBuffer = new();
        
    private readonly List<ChangeCommit> _commits = new();

    private readonly ILogManagedChangeProducer<IUpdatableSudokuSolvingState> _producer;

    private readonly IPushHandler[] _pushHandlers =
    {
        new ReturnPushHandler(), new WaitForAllPushHandler(), new ChooseBestPushHandler()
    };

    public LogManagedChangeBuffer(ILogManagedChangeProducer<IUpdatableSudokuSolvingState> changeProducer)
    {
        _producer = changeProducer;
    }

    public bool HandlesLog => true;

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
        
        _commits.Add(new ChangeCommit(ChangeBufferHelper.EstablishChangeList(_solutionAddedBuffer, _possibilityRemovedBuffer), builder));
        return true;
    }

    public void Push(ICommitMaker pusher)
    {
        if (_commits.Count == 0) return;

        var handler = _pushHandlers[(int)pusher.OnCommitBehavior];
        handler.PushWithLogsManaged(pusher, _commits, _producer);
        
        _commits.Clear();
    }

    public void PushCommit(BuiltChangeCommit commit)
    {
        var state = _producer.CurrentState;

        foreach (var change in commit.Changes)
        {
            _producer.ExecuteChange(change);
        }
        _producer.LogManager.AddFromReport(commit.Report, commit.Changes, commit.Maker, state);
    }
}

public interface IPushHandler
{
    void PushWithLogsManaged(ICommitMaker pusher, List<ChangeCommit> commits, ILogManagedChangeProducer<IUpdatableSudokuSolvingState> producer);
}

public class ReturnPushHandler : IPushHandler
{
    public void PushWithLogsManaged(ICommitMaker pusher, List<ChangeCommit> commits, ILogManagedChangeProducer<IUpdatableSudokuSolvingState> producer)
    {
        var state = producer.CurrentState;

        var commit = commits[0];
        
        foreach (var change in commit.Changes)
        { 
            producer.ExecuteChange(change);
        }

        producer.LogManager.AddFromReport(commit.Builder.Build(commit.Changes, state), commit.Changes, pusher, state);
    }
}

public class WaitForAllPushHandler : IPushHandler
{
    public void PushWithLogsManaged(ICommitMaker pusher, List<ChangeCommit> commits, ILogManagedChangeProducer<IUpdatableSudokuSolvingState> producer)
    {
        var state = producer.CurrentState;
        
        foreach (var commit in commits)
        {
            List<SolverProgress> impactfulChanges = new();
            
            foreach (var change in commit.Changes)
            {
                if (producer.ExecuteChange(change)) impactfulChanges.Add(change);
            }

            if (impactfulChanges.Count == 0) continue;
            
            producer.LogManager.AddFromReport(commit.Builder.Build(impactfulChanges, state), impactfulChanges, pusher, state);
        }
    }
}

public class ChooseBestPushHandler : IPushHandler
{
    private readonly ICustomCommitComparer _default = new DefaultCommitComparer();
    
    public void PushWithLogsManaged(ICommitMaker pusher, List<ChangeCommit> commits, ILogManagedChangeProducer<IUpdatableSudokuSolvingState> producer)
    {
        var state = producer.CurrentState;

        var best = commits[0];
        var comparer = pusher as ICustomCommitComparer ?? _default;

        for (int i = 1; i < commits.Count; i++)
        {
            if (comparer.Compare(best, commits[i]) < 0) best = commits[i];
        }
        
        foreach (var change in best.Changes)
        { 
            producer.ExecuteChange(change);
        }

        producer.LogManager.AddFromReport(best.Builder.Build(best.Changes, state), best.Changes, pusher, state);
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
        int score = 0;

        foreach (var change in first.Changes)
        {
            score += change.ProgressType == ProgressType.SolutionAddition ? SolutionAddedValue : PossibilityRemovedValue;
        }

        foreach (var change in second.Changes)
        {
            score -= change.ProgressType == ProgressType.SolutionAddition ? SolutionAddedValue : PossibilityRemovedValue;
        }

        return score;
    }
}