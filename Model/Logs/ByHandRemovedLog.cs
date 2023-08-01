using System.Collections.Generic;

namespace Model.Logs;

public class ByHandRemovedLog : ISolverLog
{
    public int Id { get; }
    public string Title => "Removed by hand";
    public Intensity Intensity => Intensity.Six;
    public string Text { get; }
    public string SolverState { get; }

    private readonly LogPart _asPart;

    public ByHandRemovedLog(int id, int possibility, int row, int col, string solverState)
    {
        Id = id;
        Text = $"[{row + 1}, {col + 1}] {possibility} removed by hand";
        SolverState = solverState;
        _asPart = new LogPart(SolverAction.PossibilityRemoved, possibility, row, col);
    }
    
    public void DefinitiveAdded(int n, int row, int col)
    {
        throw new System.NotImplementedException();
    }

    public void PossibilityRemoved(int p, int row, int col)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<LogPart> AllParts()
    {
        yield return _asPart;
    }
}