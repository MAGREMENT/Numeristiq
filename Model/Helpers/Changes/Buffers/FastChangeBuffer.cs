using System.Collections.Generic;
using Model.Sudoku.Solver.StrategiesUtility;

namespace Model.Helpers.Changes.Buffers;

public class FastChangeBuffer : IChangeBuffer
{
    private readonly HashSet<CellPossibility> _possibilityRemovedBuffer = new();
    private readonly HashSet<CellPossibility> _solutionAddedBuffer = new();

    private readonly IChangeProducer _producer;

    public FastChangeBuffer(IChangeProducer producer)
    {
        _producer = producer;
    }

    public void ProposePossibilityRemoval(CellPossibility cp)
    {
        if (_producer.CanRemovePossibility(cp)) _possibilityRemovedBuffer.Add(cp);
    }

    public void ProposeSolutionAddition(CellPossibility cp)
    {
        if (_producer.CanAddSolution(cp)) _solutionAddedBuffer.Add(cp);
    }

    public bool NotEmpty()
    {
        return _possibilityRemovedBuffer.Count > 0 || _solutionAddedBuffer.Count > 0;
    }

    public bool Commit(IChangeReportBuilder builder)
    {
        return NotEmpty();
    }

    public void Push(ICommitMaker pusher)
    {
        foreach (var solution in _solutionAddedBuffer)
        {
            _producer.ExecuteChange(new SolverChange(ChangeType.Solution, solution));
        }

        foreach (var possibility in _possibilityRemovedBuffer)
        {
            _producer.ExecuteChange(new SolverChange(ChangeType.Possibility, possibility));
        }
        
        _solutionAddedBuffer.Clear();
        _possibilityRemovedBuffer.Clear();
    }

    public void PushCommit(BuiltChangeCommit commit)
    {
        foreach (var change in commit.Changes)
        {
            _producer.ExecuteChange(change);
        }
    }
}