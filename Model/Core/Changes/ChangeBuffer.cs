using System.Collections.Generic;
using Model.Core.Highlighting;
using Model.Utility;

namespace Model.Core.Changes;

/// <summary>
/// Strategies are most often dependant on the fact that the board stay the same through the search. It is then
/// important that no change are executed outside of the Push() method, which signals that a strategy has stopped
/// searching.
/// </summary>
public class ChangeBuffer<TVerifier, THighlighter> where TVerifier : IUpdatableSolvingState
    where THighlighter : ISolvingStateHighlighter
{
    private readonly HashSet<CellPossibility> _possibilitiesRemoved = new();
    private readonly HashSet<CellPossibility> _solutionsAdded = new();

    public List<ChangeCommit<TVerifier, THighlighter>> Commits { get; } = new();

    private readonly IChangeProducer _producer;

    public ChangeBuffer(IChangeProducer producer)
    {
        _producer = producer;
    }

    public void ProposePossibilityRemoval(int possibility, Cell cell)
    {
        ProposePossibilityRemoval(new CellPossibility(cell, possibility));
    }

    public void ProposePossibilityRemoval(int possibility, int row, int col)
    {
        ProposePossibilityRemoval(new CellPossibility(row, col, possibility));
    }

    public void ProposePossibilityRemoval(CellPossibility cp)
    {
        if (_producer.CanRemovePossibility(cp)) _possibilitiesRemoved.Add(cp);
    }

    public void ProposeSolutionAddition(int number, int row, int col)
    {
        ProposeSolutionAddition(new CellPossibility(row, col, number));
    }

    public void ProposeSolutionAddition(int number, Cell cell)
    {
        ProposeSolutionAddition(new CellPossibility(cell, number));
    }

    public void ProposeSolutionAddition(CellPossibility cp)
    {
        if (_producer.CanAddSolution(cp)) _solutionsAdded.Add(cp);
    }

    public bool NotEmpty()
    {
        return _possibilitiesRemoved.Count > 0 || _solutionsAdded.Count > 0;
    }

    public bool Commit(IChangeReportBuilder<TVerifier, THighlighter> builder)
    {
        if (_producer.FastMode ||
            (_possibilitiesRemoved.Count == 0 && _solutionsAdded.Count == 0)) return false;

        Commits.Add(new ChangeCommit<TVerifier, THighlighter>(EstablishChangeList(), builder));
        return true;
    }

    public IEnumerable<SolverProgress> DumpChanges()
    {
        foreach (var solution in _solutionsAdded)
        {
            yield return new SolverProgress(ProgressType.SolutionAddition, solution);
        }
        
        foreach (var possibility in _possibilitiesRemoved)
        {
            yield return new SolverProgress(ProgressType.PossibilityRemoval, possibility);
        }
        
        _solutionsAdded.Clear();
        _possibilitiesRemoved.Clear();
    }
    
    private SolverProgress[] EstablishChangeList()
    {
        var count = 0;
        var changes = new SolverProgress[_solutionsAdded.Count + _possibilitiesRemoved.Count];
        
        foreach (var solution in _solutionsAdded)
        {
            changes[count++] = new SolverProgress(ProgressType.SolutionAddition, solution);
        }
        
        foreach (var possibility in _possibilitiesRemoved)
        {
            changes[count++] = new SolverProgress(ProgressType.PossibilityRemoval, possibility);
        }
        
        _solutionsAdded.Clear();
        _possibilitiesRemoved.Clear();

        return changes;
    }
}

public interface IChangeProducer
{
    public bool FastMode { get; }
    public bool CanRemovePossibility(CellPossibility cp);
    public bool CanAddSolution(CellPossibility cp);
}