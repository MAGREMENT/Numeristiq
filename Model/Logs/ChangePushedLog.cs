using System.Collections.Generic;
using System.ComponentModel.Design;

namespace Model.Logs;

public class ChangePushedLog : ISolverLog
{
    public int Id { get; }
    public string Title { get; }
    public Intensity Intensity { get; }

    public string Text
    {
        get
        {
            var result = "";
            foreach (var change in _changes)
            {
                result += change + "\n";
            }

            return result;
        }
    }

    public string SolverState { get; }

    private readonly IEnumerable<LogChange> _changes;
    private readonly IEnumerable<LogCause> _causes;

    public ChangePushedLog(int id, IStrategy strategy, IEnumerable<LogChange> changes, IEnumerable<LogCause> causes,
        string solverState)
    {
        Id = id;
        Title = strategy.Name;
        Intensity = (Intensity)strategy.Difficulty;
        SolverState = solverState;
        _changes = changes;
        _causes = causes;
    }

    public IEnumerable<LogChange> AllChanges()
    {
        return _changes;
    }

    public IEnumerable<LogCause> AllCauses()
    {
        return _causes;
    }
}