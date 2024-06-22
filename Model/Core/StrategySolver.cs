using System;
using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Steps;
using Model.Core.Trackers;
using Model.Sudokus.Solver;

namespace Model.Core;

public abstract class StrategySolver<TStrategy, TSolvingState, THighlighter, TChange, TChangeBuffer, TStep> 
    : ITrackerAttachable<TStrategy, TSolvingState>, ISolveResult<TSolvingState>
    where TStrategy : Strategy where TChangeBuffer : IChangeBuffer<TChange, TSolvingState, THighlighter>
    where TStep : IStep
{
    protected TSolvingState? _currentState;
    protected readonly List<TStep> _steps = new();
    
    public bool StartedSolving { get; private set; }
    public StrategyManager<TStrategy> StrategyManager { get; init; } = new();

    public TSolvingState CurrentState
    {
        get
        {
            _currentState ??= GetSolvingState();
            return _currentState;
        }
    }

    public TSolvingState? StartState { get; protected set; }

    /// <summary>
    /// Disables steps & instance handling
    /// </summary>
    public bool FastMode { get; set; }
    public TChangeBuffer ChangeBuffer { get; }
    public IReadOnlyList<TStep> Steps => _steps;

    public event OnSolveStart? SolveStarted;
    public event OnStrategyStart<TStrategy>? StrategyStarted;
    public event OnStrategyEnd<TStrategy>? StrategyEnded;
    public event OnSolveDone<ISolveResult<TSolvingState>>? SolveDone;

    protected StrategySolver()
    {
        ChangeBuffer = GetChangeBuffer();
    }
    
    public void Solve(bool stopAtProgress = false)
    {
        StartedSolving = true;
        SolveStarted?.Invoke();
        var solutionAdded = 0;
        var possibilityRemoved = 0;
        
        for (int i = 0; i < StrategyManager.Strategies.Count; i++)
        {
            var current = StrategyManager.Strategies[i];
            if (!current.Enabled) continue;

            StrategyStarted?.Invoke(current, i);
            ApplyStrategy(current);
            OnStrategyEnd(current, ref solutionAdded, ref possibilityRemoved);
            StrategyEnded?.Invoke(current, i, solutionAdded, possibilityRemoved);

            if (solutionAdded + possibilityRemoved == 0) continue;
            
            OnChangeMade();
            i = -1;
            solutionAdded = 0;
            possibilityRemoved = 0;

            if (stopAtProgress || IsComplete()) break;
        }

        SolveDone?.Invoke(this);
    }
    
    public IReadOnlyList<BuiltChangeCommit<TChange, THighlighter>> EveryPossibleNextStep()
    {
        List<BuiltChangeCommit<TChange, THighlighter>> result = new();
        
        for (int i = 0; i < StrategyManager.Strategies.Count; i++)
        {
            var current = StrategyManager.Strategies[i];
            if (!current.Enabled) continue;
            
            StrategyStarted?.Invoke(current, i);
            ApplyStrategy(current);
            result.AddRange(GetCommits(current));
            StrategyEnded?.Invoke(current, i, 0, 0);
        }
        
        return result;
    }
    
    public Clue<THighlighter>? NextClue()
    {
        Clue<THighlighter>? result = null;
        
        for (int i = 0; i < StrategyManager.Strategies.Count; i++)
        {
            var current = StrategyManager.Strategies[i];
            if (!current.Enabled) continue;

            StrategyStarted?.Invoke(current, i);
            ApplyStrategy(current);
            if (ChangeBuffer.Commits.Count != 0)
            {
                var state = CurrentState;
                var commit = ChangeBuffer.Commits[0];
                result = commit.Builder.BuildClue(commit.Changes, state);
            }
            StrategyEnded?.Invoke(current, i, 0, 0);

            if (result is not null) break;
        }
        
        ChangeBuffer.Commits.Clear();
        return result;
    }
    
    public void ApplyCommit(BuiltChangeCommit<TChange, THighlighter> commit)
    {
        StartedSolving = true;
        var state = CurrentState;
        var didSomething = false;
        foreach (var change in commit.Changes)
        {
            if (ExecuteChange(change)) didSomething = true;
        }
        
        if(didSomething && !FastMode) 
            AddStepFromReport(commit.Report, commit.Changes, commit.Maker, state);
    }
    
    public IEnumerable<TStrategy> EnumerateStrategies()
    {
        return StrategyManager.Strategies;
    }
    
    protected void OnNewSolvable()
    {
        StartedSolving = false;
        StartState = GetSolvingState();
        _steps.Clear();
    }
    
    protected abstract TSolvingState GetSolvingState();
    public abstract bool IsResultCorrect();
    public abstract bool HasSolverFailed();
    
    protected abstract void OnChangeMade();
    protected abstract void ApplyStrategy(TStrategy strategy);
    protected abstract bool IsComplete();
    protected abstract bool ExecuteChange(TChange change, ref int solutionAdded, ref int possibilitiesRemoved);
    protected abstract bool ExecuteChange(TChange change);
    protected abstract void AddStepFromReport(ChangeReport<THighlighter> report, IReadOnlyList<TChange> changes,
        Strategy maker, TSolvingState stateBefore);
    protected abstract ICommitComparer<TChange> GetDefaultCommitComparer();
    protected abstract TChangeBuffer GetChangeBuffer();
    
    private void OnStrategyEnd(Strategy strategy, ref int solutionAdded, ref int possibilitiesRemoved)
    {
        if (FastMode)
        {
            foreach (var change in ChangeBuffer.DumpChanges())
            {
                ExecuteChange(change, ref solutionAdded, ref possibilitiesRemoved);
            }
        }
        else
        {
            if (ChangeBuffer.Commits.Count == 0) return;
            
            HandleCommits<TSolvingState, THighlighter, TChange> handler = strategy.InstanceHandling switch
            {
                InstanceHandling.FirstOnly => HandleFirstOnly,
                InstanceHandling.UnorderedAll => HandleUnorderedAll,
                InstanceHandling.BestOnly => HandleBestOnly,
                InstanceHandling.SortedAll => HandleSortedAll,
                _ => throw new Exception()
            };

            handler(strategy, ChangeBuffer.Commits, ref solutionAdded, ref possibilitiesRemoved);
            ChangeBuffer.Commits.Clear();
        }
    }

    private IEnumerable<BuiltChangeCommit<TChange, THighlighter>> GetCommits(Strategy strategy)
    {
        if (ChangeBuffer.Commits.Count == 0) yield break;
        
        var state = CurrentState;
        foreach (var commit in ChangeBuffer.Commits)
        {
            yield return new BuiltChangeCommit<TChange, THighlighter>(strategy, commit.Changes,
                commit.Builder.BuildReport(commit.Changes, state));
        }

        ChangeBuffer.Commits.Clear();
    }
    
    private void HandleFirstOnly(Strategy pusher, List<ChangeCommit<TChange, TSolvingState, THighlighter>> commits,
        ref int solutionAdded, ref int possibilitiesRemoved)
    {
        var state = CurrentState;
        var commit = commits[0];
        
        foreach (var change in commit.Changes)
        { 
            ExecuteChange(change, ref solutionAdded, ref possibilitiesRemoved);
        }

        AddStepFromReport(commit.Builder.BuildReport(commit.Changes, state), commit.Changes, pusher, state);
    }
    
    private void HandleUnorderedAll(Strategy pusher, List<ChangeCommit<TChange, TSolvingState, THighlighter>> commits,
        ref int solutionAdded, ref int possibilitiesRemoved)
    {
        var state = CurrentState;
        
        foreach (var commit in commits)
        {
            List<TChange> impactfulChanges = new();
            
            foreach (var change in commit.Changes)
            {
                if (ExecuteChange(change, ref solutionAdded, ref possibilitiesRemoved)) impactfulChanges.Add(change);
            }

            if (impactfulChanges.Count == 0) continue;
            
            AddStepFromReport(commit.Builder.BuildReport(impactfulChanges, state), impactfulChanges, pusher, state);
        }
    }
    
    private void HandleBestOnly(Strategy pusher, List<ChangeCommit<TChange, TSolvingState, THighlighter>> commits,
        ref int solutionAdded, ref int possibilitiesRemoved)
    {
        var state = CurrentState;

        var best = commits[0];
        var comparer = pusher as ICommitComparer<TChange> ?? GetDefaultCommitComparer();

        for (int i = 1; i < commits.Count; i++)
        {
            if (comparer.Compare(best, commits[i]) < 0) best = commits[i];
        }
        
        foreach (var change in best.Changes)
        { 
            ExecuteChange(change, ref solutionAdded, ref possibilitiesRemoved);
        }

        AddStepFromReport(best.Builder.BuildReport(best.Changes, state), best.Changes, pusher, state);
    }
    
    private void HandleSortedAll(Strategy pusher, List<ChangeCommit<TChange, TSolvingState, THighlighter>> commits,
        ref int solutionAdded, ref int possibilitiesRemoved)
    {
        var state = CurrentState;
        var comparer = pusher as ICommitComparer<TChange> ?? GetDefaultCommitComparer();
        commits.Sort((c1, c2) => comparer.Compare(c1, c2));

        foreach (var commit in commits)
        {
            List<TChange> impactfulChanges = new();
            
            foreach (var change in commit.Changes)
            {
                if (ExecuteChange(change, ref solutionAdded, ref possibilitiesRemoved)) impactfulChanges.Add(change);
            }

            if (impactfulChanges.Count == 0) continue;
            
            AddStepFromReport(commit.Builder.BuildReport(impactfulChanges, state), impactfulChanges, pusher, state);
        }
    }
}

public interface ISolveResult<out TSolvingState>
{ 
    public TSolvingState? StartState { get; }
    public bool IsResultCorrect();
    public bool HasSolverFailed();
}

public delegate void HandleCommits<TSolvingState, THighlighter, TChange>(Strategy pusher, List<ChangeCommit<TChange, TSolvingState,
    THighlighter>> commits, ref int solutionAdded, ref int possibilitiesRemoved);