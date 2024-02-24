using System.Collections.Generic;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Helpers.Changes;

/// <summary>
/// Strategies are most often dependant on the fact that the board stay the same through the search. It is then
/// important that no change are executed outside of the Push() method, which signals that a strategy has stopped
/// searching.
/// </summary>
public interface IChangeBuffer
{
    public bool HandlesLog { get; }
    
    public void ProposePossibilityRemoval(int possibility, Cell cell)
    {
        ProposePossibilityRemoval(new CellPossibility(cell, possibility));
    }

    public void ProposePossibilityRemoval(int possibility, int row, int col)
    {
        ProposePossibilityRemoval(new CellPossibility(row, col, possibility));
    }
    
    public void ProposePossibilityRemoval(CellPossibility cp);
    
    public void ProposeSolutionAddition(int number, int row, int col)
    {
        ProposeSolutionAddition(new CellPossibility(row, col, number));
    }
    
    public void ProposeSolutionAddition(int number, Cell cell)
    {
        ProposeSolutionAddition(new CellPossibility(cell, number));
    }

    public void ProposeSolutionAddition(CellPossibility cp);

    public bool NotEmpty();

    public bool Commit(IChangeReportBuilder builder);

    public void Push(ICommitMaker pusher);

    public void PushCommit(BuiltChangeCommit commit);
}

public static class ChangeBufferHelper
{
    public static SolverChange[] EstablishChangeList(HashSet<CellPossibility> solutionsAdded, HashSet<CellPossibility> possibilitiesRemoved)
    {
        var count = 0;
        var changes = new SolverChange[solutionsAdded.Count + possibilitiesRemoved.Count];
        
        foreach (var solution in solutionsAdded)
        {
            changes[count++] = new SolverChange(ChangeType.Solution, solution);
        }
        
        foreach (var possibility in possibilitiesRemoved)
        {
            changes[count++] = new SolverChange(ChangeType.Possibility, possibility);
        }
        
        solutionsAdded.Clear();
        possibilitiesRemoved.Clear();

        return changes;
    }
}

public class ChangeCommit
{
    public SolverChange[] Changes { get; }
    public IChangeReportBuilder Builder { get; }

    public ChangeCommit(SolverChange[] changes, IChangeReportBuilder builder)
    {
        Changes = changes;
        Builder = builder;
    }
}

public class BuiltChangeCommit
{
    public BuiltChangeCommit(ICommitMaker maker, SolverChange[] changes, ChangeReport report)
    {
        Maker = maker;
        Changes = changes;
        Report = report;
    }
    
    public ICommitMaker Maker { get; }
    public SolverChange[] Changes { get; }
    public ChangeReport Report { get; }
}