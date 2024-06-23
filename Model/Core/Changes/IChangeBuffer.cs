using System.Collections.Generic;
using Model.Core.Highlighting;
using Model.Utility;

namespace Model.Core.Changes;

/// <summary>
/// Strategies are most often dependant on the fact that the board stay the same through the search. It is then
/// important that no change are executed outside during a search
/// </summary>
public interface IChangeBuffer<TChange, TVerifier, THighlighter>
{ 
    List<ChangeCommit<TChange, TVerifier, THighlighter>> Commits { get; }

    bool NotEmpty();
    bool Commit(IChangeReportBuilder<TChange, TVerifier, THighlighter> builder);
    IEnumerable<TChange> DumpChanges();
}

public class DichotomousChangeBuffer<TVerifier, THighlighter> : IChangeBuffer<DichotomousChange, TVerifier, THighlighter>
{
    private readonly HashSet<Cell> _possibilitiesRemoved = new();
    private readonly HashSet<Cell> _solutionsAdded = new();
    
    private readonly IDichotomousChangeProducer _producer;
    
    public List<ChangeCommit<DichotomousChange, TVerifier, THighlighter>> Commits { get; } = new();

    public DichotomousChangeBuffer(IDichotomousChangeProducer producer)
    {
        _producer = producer;
    }

    public void ProposePossibilityRemoval(Cell cell)
    {
        if (_producer.CanRemovePossibility(cell)) _possibilitiesRemoved.Add(cell);
    }

    public void ProposeSolutionAddition(Cell cell)
    {
        if (_producer.CanAddSolution(cell)) _solutionsAdded.Add(cell);
    }
    
    public bool NotEmpty()
    {
        return _possibilitiesRemoved.Count > 0 || _solutionsAdded.Count > 0;
    }

    public bool Commit(IChangeReportBuilder<DichotomousChange, TVerifier, THighlighter> builder)
    {
        if (_producer.FastMode ||
            (_possibilitiesRemoved.Count == 0 && _solutionsAdded.Count == 0)) return false;

        Commits.Add(new ChangeCommit<DichotomousChange, TVerifier, THighlighter>(EstablishChangeList(), builder));
        return true;
    }

    public IEnumerable<DichotomousChange> DumpChanges()
    {
        foreach (var solution in _solutionsAdded)
        {
            yield return new DichotomousChange(ChangeType.SolutionAddition, solution);
        }
        
        foreach (var possibility in _possibilitiesRemoved)
        {
            yield return new DichotomousChange(ChangeType.PossibilityRemoval, possibility);
        }
        
        _solutionsAdded.Clear();
        _possibilitiesRemoved.Clear();
    }
    
    private DichotomousChange[] EstablishChangeList()
    {
        var count = 0;
        var changes = new DichotomousChange[_solutionsAdded.Count + _possibilitiesRemoved.Count];
        
        foreach (var solution in _solutionsAdded)
        {
            changes[count++] = new DichotomousChange(ChangeType.SolutionAddition, solution);
        }
        
        foreach (var possibility in _possibilitiesRemoved)
        {
            changes[count++] = new DichotomousChange(ChangeType.PossibilityRemoval, possibility);
        }
        
        _solutionsAdded.Clear();
        _possibilitiesRemoved.Clear();

        return changes;
    }
}

public class IChangeBuffer<TVerifier, THighlighter> : IChangeBuffer<NumericChange, TVerifier, THighlighter>
    where TVerifier : IUpdatableNumericSolvingState
    where THighlighter : INumericSolvingStateHighlighter
{
    private readonly HashSet<CellPossibility> _possibilitiesRemoved = new();
    private readonly HashSet<CellPossibility> _solutionsAdded = new();

    public List<ChangeCommit<NumericChange, TVerifier, THighlighter>> Commits { get; } = new();

    private readonly INumericChangeProducer _producer;

    public IChangeBuffer(INumericChangeProducer producer)
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

    public bool Commit(IChangeReportBuilder<NumericChange, TVerifier, THighlighter> builder)
    {
        if (_producer.FastMode ||
            (_possibilitiesRemoved.Count == 0 && _solutionsAdded.Count == 0)) return false;

        Commits.Add(new ChangeCommit<NumericChange, TVerifier, THighlighter>(EstablishChangeList(), builder));
        return true;
    }

    public IEnumerable<NumericChange> DumpChanges()
    {
        foreach (var solution in _solutionsAdded)
        {
            yield return new NumericChange(ChangeType.SolutionAddition, solution);
        }
        
        foreach (var possibility in _possibilitiesRemoved)
        {
            yield return new NumericChange(ChangeType.PossibilityRemoval, possibility);
        }
        
        _solutionsAdded.Clear();
        _possibilitiesRemoved.Clear();
    }
    
    private NumericChange[] EstablishChangeList()
    {
        var count = 0;
        var changes = new NumericChange[_solutionsAdded.Count + _possibilitiesRemoved.Count];
        
        foreach (var solution in _solutionsAdded)
        {
            changes[count++] = new NumericChange(ChangeType.SolutionAddition, solution);
        }
        
        foreach (var possibility in _possibilitiesRemoved)
        {
            changes[count++] = new NumericChange(ChangeType.PossibilityRemoval, possibility);
        }
        
        _solutionsAdded.Clear();
        _possibilitiesRemoved.Clear();

        return changes;
    }
}

public interface IDichotomousChangeProducer
{
    public bool FastMode { get; }
    public bool CanRemovePossibility(Cell cell);
    public bool CanAddSolution(Cell cell);
}

public interface INumericChangeProducer
{
    public bool FastMode { get; }
    public bool CanRemovePossibility(CellPossibility cp);
    public bool CanAddSolution(CellPossibility cp);
}
