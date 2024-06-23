using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Steps;
using Model.Utility;

namespace Model.Core;

public abstract class DichotomousStrategySolver<TStrategy, TSolvingState, THighlighter> : 
    StrategySolver<TStrategy, TSolvingState, THighlighter, DichotomousChange,
        DichotomousChangeBuffer<TSolvingState, THighlighter>, IDichotomousStep<THighlighter>>, IDichotomousChangeProducer
    where TSolvingState : IUpdatableDichotomousSolvingState where TStrategy : Strategy
{
    public override DichotomousChangeBuffer<TSolvingState, THighlighter> ChangeBuffer { get; }

    protected DichotomousStrategySolver()
    {
        ChangeBuffer = new DichotomousChangeBuffer<TSolvingState, THighlighter>(this);
    }

    public abstract bool CanRemovePossibility(Cell cell);
    public abstract bool CanAddSolution(Cell cell);
    
    protected abstract bool AddSolution(int row, int col);
    protected abstract bool RemovePossibility(int row, int col);
    
    protected override bool ExecuteChange(DichotomousChange progress)
    {
        return progress.Type == ChangeType.PossibilityRemoval 
            ? RemovePossibility(progress.Row, progress.Column) 
            : AddSolution(progress.Row, progress.Column);
    }
    
    protected override bool ExecuteChange(DichotomousChange progress, ref int solutionAdded, ref int possibilitiesRemoved)
    {
        if (progress.Type == ChangeType.SolutionAddition)
        {
            if (AddSolution(progress.Row, progress.Column))
            {
                solutionAdded++;
                return true;
            }
        }
        else if (RemovePossibility(progress.Row, progress.Column))
        {
            possibilitiesRemoved++;
            return true;
        }

        return false;
    }

    protected override ICommitComparer<DichotomousChange> GetDefaultCommitComparer()
    {
        return DefaultDichotomousCommitComparer.Instance;
    }
    
    protected override void AddStepFromReport(ChangeReport<THighlighter> report, IReadOnlyList<DichotomousChange> changes,
        Strategy maker, TSolvingState stateBefore)
    {
        //TODO
    }
}