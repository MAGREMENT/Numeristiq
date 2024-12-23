using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Core.Steps;
using Model.Utility;

namespace Model.Core;

public abstract class NumericStrategySolver<TStrategy, TSolvingState, THighlighter> : 
    StrategySolver<TStrategy, TSolvingState, THighlighter, NumericChange,
        NumericChangeBuffer<TSolvingState, THighlighter>, IStep<THighlighter, TSolvingState, NumericChange>>, INumericChangeProducer
    where TSolvingState : INumericSolvingState where THighlighter : INumericSolvingStateHighlighter where TStrategy : Strategy
{
    protected int _solutionCount;

    public override NumericChangeBuffer<TSolvingState, THighlighter> ChangeBuffer { get; }

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
    
    public abstract bool CanRemovePossibility(CellPossibility cp);
    public abstract bool CanAddSolution(CellPossibility cp);

    protected void OnNewSolvable(int solutionCount)
    {
        OnNewSolvable();
        _solutionCount = solutionCount;
    }
    
    protected abstract bool AddSolution(int number, int row, int col);
    protected abstract bool RemoveSolution(int row, int col);
    protected abstract bool RemovePossibility(int possibility, int row, int col);

    protected override bool ExecuteChange(NumericChange progress)
    {
        if (progress.Type == ChangeType.PossibilityRemoval)
            return RemovePossibility(progress.Number, progress.Row, progress.Column);
        
        if (!AddSolution(progress.Number, progress.Row, progress.Column)) return false;
        
        _solutionCount++;
        return true;
    }
    
    protected override bool ExecuteChange(NumericChange progress, ref int solutionAdded, ref int possibilitiesRemoved)
    {
        if (progress.Type == ChangeType.SolutionAddition)
        {
            if (AddSolution(progress.Number, progress.Row, progress.Column))
            {
                solutionAdded++;
                _solutionCount++;
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

    protected override ICommitComparer<NumericChange> GetDefaultCommitComparer()
    {
        return DefaultNumericCommitComparer.Instance;
    }

    protected override void AddStepFromReport(ChangeReport<THighlighter> report, IReadOnlyList<NumericChange> changes,
        Strategy maker, TSolvingState stateBefore)
    {
        _steps.Add(new ChangeReportStep<THighlighter, TSolvingState, NumericChange>(_steps.Count + 1, maker, changes, report, stateBefore,
            ApplyChangesToState(stateBefore, changes)));
    }

    private void AddStepByHand(int possibility, int row, int col, ChangeType changeType,
        TSolvingState stateBefore)
    {
        _steps.Add(new ByHandNumericStep<THighlighter, TSolvingState>(_steps.Count + 1, possibility, row, col, changeType, stateBefore,
            ApplyChangesToState(stateBefore, new []{new NumericChange(changeType, possibility, row, col)})));
    }
}