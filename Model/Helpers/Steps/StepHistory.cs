using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;

namespace Model.Helpers.Steps;

public class StepHistory<THighlighter> where THighlighter : ISolvingStateHighlighter
{
    private readonly List<ISolverStep<THighlighter>> _steps = new();

    public IReadOnlyList<ISolverStep<THighlighter>> Steps => _steps;
    private int _idCount = 1;
    
    public void Clear()
    {
        _idCount = 1;
        _steps.Clear();
    }

    public void AddFromReport(ChangeReport<THighlighter> report, IReadOnlyList<SolverProgress> changes, ICommitMaker maker, IUpdatableSolvingState stateBefore)
    {
        _steps.Add(new ChangeReportStep<THighlighter>(_idCount++, maker, changes, report, stateBefore));
    }

    public void AddByHand(int possibility, int row, int col, ProgressType progressType, IUpdatableSolvingState stateBefore)
    {
        _steps.Add(new ByHandStep<THighlighter>(_idCount++, possibility, row, col, progressType, stateBefore));
    }
}