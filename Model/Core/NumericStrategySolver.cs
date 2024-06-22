using System;
using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Core.Steps;
using Model.Sudokus.Solver;
using Model.Utility;

namespace Model.Core;

public abstract class NumericStrategySolver<TStrategy, TSolvingState, THighlighter> : INumericChangeProducer,
    ITrackerAttachable<TStrategy, TSolvingState>, ISolveResult<TSolvingState>
    where TStrategy : Strategy where TSolvingState : IUpdatableSolvingState where THighlighter : ISolvingStateHighlighter
{
    protected int _solutionCount;
    protected TSolvingState? _currentState;
    private readonly List<INumericStep<THighlighter>> _steps = new();

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
    public NumericChangeBuffer<TSolvingState, THighlighter> ChangeBuffer { get; }
    public IReadOnlyList<INumericStep<THighlighter>> Steps => _steps;

    public event OnSolveStart? SolveStarted;
    public event OnStrategyStart<TStrategy>? StrategyStarted;
    public event OnStrategyEnd<TStrategy>? StrategyEnded;
    public event OnSolveDone<ISolveResult<TSolvingState>>? SolveDone;

    protected NumericStrategySolver()
    {
        ChangeBuffer = new NumericChangeBuffer<TSolvingState, THighlighter>(this);
    }
    
    public void SetSolutionByHand(int number, int row, int col)
    {
        if (!CanAddSolution(new CellPossibility(row, col, number))) RemoveSolution(row, col);

        var before = CurrentState;
        if (!AddSolution(number, row, col)) return;

        if (!StartedSolving) StartState = GetSolvingState();
        else if (!FastMode) AddStepByHand(number, row, col, ChangeType.SolutionAddition, before);
    }

    public void RemoveSolutionByHand(int row, int col)
    {
        if (StartedSolving) return;

        RemoveSolution(row, col);
        StartState = GetSolvingState();
    }

    public void RemovePossibilityByHand(int possibility, int row, int col)
    {
        if (StartedSolving && !FastMode)
        {
            var stateBefore = CurrentState;
            if (!RemovePossibility(possibility, row, col)) return;
            AddStepByHand(possibility, row, col, ChangeType.PossibilityRemoval, stateBefore);
        }
        else if (!RemovePossibility(possibility, row, col)) return;

        if (!StartedSolving) StartState = GetSolvingState();
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

            _solutionCount += solutionAdded;
            OnChangeMade();
            i = -1;
            solutionAdded = 0;
            possibilityRemoved = 0;

            if (stopAtProgress || IsComplete()) break;
        }

        SolveDone?.Invoke(this);
    }
    
    public IReadOnlyList<BuiltChangeCommit<NumericChange, THighlighter>> EveryPossibleNextStep()
    {
        List<BuiltChangeCommit<NumericChange, THighlighter>> result = new();
        
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
    
    public void ApplyCommit(BuiltChangeCommit<NumericChange, THighlighter> commit)
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
    
    public abstract bool CanRemovePossibility(CellPossibility cp);
    public abstract bool CanAddSolution(CellPossibility cp);

    protected void OnNewSolvable(int solutionCount)
    {
        StartedSolving = false;
        StartState = GetSolvingState();
        _solutionCount = solutionCount;
        _steps.Clear();
    }

    protected abstract TSolvingState GetSolvingState();
    public abstract bool IsResultCorrect();
    public abstract bool HasSolverFailed();
    protected abstract bool AddSolution(int number, int row, int col);
    protected abstract bool RemoveSolution(int row, int col);
    protected abstract bool RemovePossibility(int possibility, int row, int col);
    protected abstract void OnChangeMade();
    protected abstract void ApplyStrategy(TStrategy strategy);
    protected abstract bool IsComplete();

    private bool ExecuteChange(NumericChange progress)
    {
        return progress.Type == ChangeType.SolutionAddition 
            ? AddSolution(progress.Number, progress.Row, progress.Column) 
            : RemovePossibility(progress.Number, progress.Row, progress.Column);
    }
    
    private bool ExecuteChange(NumericChange progress, ref int solutionAdded, ref int possibilitiesRemoved)
    {
        if (progress.Type == ChangeType.SolutionAddition)
        {
            if (AddSolution(progress.Number, progress.Row, progress.Column))
            {
                solutionAdded++;
                return true;
            }
        }
        else if (RemovePossibility(progress.Number, progress.Row, progress.Column))
        {
            possibilitiesRemoved++;
            return true;
        }

        return false;
    }
    
    private void AddStepFromReport(ChangeReport<THighlighter> report, IReadOnlyList<NumericChange> changes,
        Strategy maker, IUpdatableSolvingState stateBefore)
    {
        _steps.Add(new ChangeReportNumericStep<THighlighter>(_steps.Count + 1, maker, changes, report, stateBefore));
    }

    private void AddStepByHand(int possibility, int row, int col, ChangeType changeType,
        IUpdatableSolvingState stateBefore)
    {
        _steps.Add(new ByHandNumericStep<THighlighter>(_steps.Count + 1, possibility, row, col, changeType, stateBefore));
    }

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
            
            HandleCommits<TSolvingState, THighlighter> handler = strategy.InstanceHandling switch
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

    private IEnumerable<BuiltChangeCommit<NumericChange, THighlighter>> GetCommits(Strategy strategy)
    {
        if (ChangeBuffer.Commits.Count == 0) yield break;
        
        var state = CurrentState;
        foreach (var commit in ChangeBuffer.Commits)
        {
            yield return new BuiltChangeCommit<NumericChange, THighlighter>(strategy, commit.Changes,
                commit.Builder.BuildReport(commit.Changes, state));
        }

        ChangeBuffer.Commits.Clear();
    }
    
    private void HandleFirstOnly(Strategy pusher, List<ChangeCommit<NumericChange, TSolvingState, THighlighter>> commits,
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
    
    private void HandleUnorderedAll(Strategy pusher, List<ChangeCommit<NumericChange, TSolvingState, THighlighter>> commits,
        ref int solutionAdded, ref int possibilitiesRemoved)
    {
        var state = CurrentState;
        
        foreach (var commit in commits)
        {
            List<NumericChange> impactfulChanges = new();
            
            foreach (var change in commit.Changes)
            {
                if (ExecuteChange(change, ref solutionAdded, ref possibilitiesRemoved)) impactfulChanges.Add(change);
            }

            if (impactfulChanges.Count == 0) continue;
            
            AddStepFromReport(commit.Builder.BuildReport(impactfulChanges, state), impactfulChanges, pusher, state);
        }
    }
    
    private void HandleBestOnly(Strategy pusher, List<ChangeCommit<NumericChange, TSolvingState, THighlighter>> commits,
        ref int solutionAdded, ref int possibilitiesRemoved)
    {
        var state = CurrentState;

        var best = commits[0];
        var comparer = pusher as ICommitComparer ??  DefaultCommitComparer.Instance;

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
    
    private void HandleSortedAll(Strategy pusher, List<ChangeCommit<NumericChange, TSolvingState, THighlighter>> commits,
        ref int solutionAdded, ref int possibilitiesRemoved)
    {
        var state = CurrentState;
        var comparer = pusher as ICommitComparer ?? DefaultCommitComparer.Instance;
        commits.Sort((c1, c2) => comparer.Compare(c1, c2));

        foreach (var commit in commits)
        {
            List<NumericChange> impactfulChanges = new();
            
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

public interface ITrackerAttachable<out TStrategy, out TSolvingState> where TStrategy : Strategy
{
    public event OnSolveStart? SolveStarted;
    public event OnStrategyStart<TStrategy>? StrategyStarted;
    public event OnStrategyEnd<TStrategy>? StrategyEnded;
    public event OnSolveDone<ISolveResult<TSolvingState>>? SolveDone;

    public IEnumerable<TStrategy> EnumerateStrategies();
}

public delegate void OnSolveStart();
public delegate void OnStrategyStart<in TStrategy>(TStrategy strategy, int index) where TStrategy : Strategy;
public delegate void OnStrategyEnd<in TStrategy>(TStrategy strategy, int index, int solutionAdded, int possibilitiesRemoved) where TStrategy : Strategy;
public delegate void OnSolveDone<in TResult>(TResult result);
public delegate void HandleCommits<TSolvingState, THighlighter>(Strategy pusher, List<ChangeCommit<NumericChange, TSolvingState,
    THighlighter>> commits, ref int solutionAdded, ref int possibilitiesRemoved)
    where TSolvingState : IUpdatableSolvingState where THighlighter : ISolvingStateHighlighter;