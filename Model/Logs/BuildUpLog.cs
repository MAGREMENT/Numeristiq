using System;
using System.Collections.Generic;

namespace Model.Logs;

public class BuildUpLog : ISolverLog
{
    private List<int> _changes = new();

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
                result += ChangeAsString(change) + "\n";
            }

            return result;
        }
    }

    public string SolverState { get; }

    public BuildUpLog(int id, IStrategy causedBy, string solverState)
    {
        Id = id;
        Title = causedBy.Name;
        Intensity = (Intensity) causedBy.Difficulty;
        SolverState = solverState;
    }

    public void DefinitiveAdded(int n, int row, int col)
    {
        _changes.Add(col + row * 9 + (n - 1) * 81);
    }

    public void PossibilityRemoved(int p, int row, int col)
    {
        _changes.Add((col + row * 9 + (p - 1) * 81) * -1);
    }

    public IEnumerable<LogChange> AllChanges()
    {
        foreach (var n in _changes)
        {
            int abs = Math.Abs(n);
            int a = abs % 81;
            yield return new LogChange(n > 0 ? SolverNumberType.Definitive : SolverNumberType.Possibility,
                abs / 81 + 1, a / 9, a % 9);
        }
    }

    public IEnumerable<LogCause> AllCauses()
    {
        yield break;
    }

    private string ChangeAsString(int n)
    {
        string action = n > 0 ? "added as definitive" : "removed from possibilities";
        int abs = Math.Abs(n);
        int a = abs % 81;
        return $"[{a / 9 + 1}, {a % 9 + 1}] {abs / 81 + 1} {action}";
    }
}