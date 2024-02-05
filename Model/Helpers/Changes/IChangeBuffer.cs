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

    public void Push(IStrategy pusher);
}

public class ChangeCommit
{
    public SolverChange[] Changes { get; }
    public IChangeReportBuilder? Builder { get; }

    public ChangeCommit(SolverChange[] changes, IChangeReportBuilder builder)
    {
        Changes = changes;
        Builder = builder;
    }

    public ChangeCommit(SolverChange[] changes)
    {
        Changes = changes;
        Builder = null;
    }
}

public class BuiltChangeCommit
{
    public BuiltChangeCommit(SolverChange[] changes, ChangeReport report)
    {
        Changes = changes;
        Report = report;
    }
    
    public SolverChange[] Changes { get; }
    public ChangeReport Report { get; }
}