using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Steps;
using Model.Utility;

namespace Model.Core;

public abstract class BinaryStrategySolver<TStrategy, TSolvingState, THighlighter> : 
    StrategySolver<TStrategy, TSolvingState, THighlighter, BinaryChange,
        BinaryChangeBuffer<TSolvingState, THighlighter>, IBinaryStep<THighlighter>>, IBinaryChangeProducer
    where TSolvingState : IBinarySolvingState where TStrategy : Strategy
{
    public override BinaryChangeBuffer<TSolvingState, THighlighter> ChangeBuffer { get; }

    protected BinaryStrategySolver()
    {
        ChangeBuffer = new BinaryChangeBuffer<TSolvingState, THighlighter>(this);
    }
    
    public abstract bool CanAddSolution(CellPossibility cell);
    
    protected abstract bool AddSolution(int number, int row, int col);
    
    protected override bool ExecuteChange(BinaryChange progress)
    {
        return AddSolution(progress.Number, progress.Row, progress.Column);
    }
    
    protected override bool ExecuteChange(BinaryChange progress, ref int solutionAdded, ref int possibilitiesRemoved)
    {
        if (!AddSolution(progress.Number, progress.Row, progress.Column)) return false;
        
        solutionAdded++;
        return true;
    }

    protected override ICommitComparer<BinaryChange> GetDefaultCommitComparer()
    {
        return DefaultBinaryCommitComparer.Instance;
    }
    
    protected override void AddStepFromReport(ChangeReport<THighlighter> report, IReadOnlyList<BinaryChange> changes,
        Strategy maker, TSolvingState stateBefore)
    {
        _steps.Add(new ChangeReportBinaryStep<THighlighter>(_steps.Count + 1, maker, changes, report,
            stateBefore, ApplyChangesToState(stateBefore, changes)));
    }

}