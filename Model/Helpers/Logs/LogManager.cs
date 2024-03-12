using System.Collections.Generic;
using Model.Helpers.Changes;

namespace Model.Helpers.Logs;

public class LogManager<THighlighter>
{
    public List<ISolverLog<THighlighter>> Logs { get; } = new();
    private int _idCount = 1;
    
    public void Clear()
    {
        _idCount = 1;
        Logs.Clear();
    }

    public void AddFromReport(ChangeReport<THighlighter> report, IReadOnlyList<SolverProgress> changes, ICommitMaker maker, IUpdatableSolvingState stateBefore)
    {
        Logs.Add(new ChangeReportLog<THighlighter>(_idCount++, maker, changes, report, stateBefore));
    }

    public void AddByHand(int possibility, int row, int col, ProgressType progressType, IUpdatableSolvingState stateBefore)
    {
        Logs.Add(new ByHandLog<THighlighter>(_idCount++, possibility, row, col, progressType, stateBefore));
    }
}