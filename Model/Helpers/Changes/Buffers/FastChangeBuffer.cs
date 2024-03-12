using System.Collections.Generic;
using Model.Sudoku.Solver.StrategiesUtility;

namespace Model.Helpers.Changes.Buffers;

public class FastChangeBuffer<TVerifier, THighlighter> : IChangeBuffer<TVerifier, THighlighter> where TVerifier : IUpdatableSolvingState
{
    private readonly HashSet<CellPossibility> _possibilityRemovedBuffer = new();
    private readonly HashSet<CellPossibility> _solutionAddedBuffer = new();

    private readonly IChangeProducer _producer;

    public FastChangeBuffer(IChangeProducer producer)
    {
        _producer = producer;
    }

    public bool HandlesLog => false;

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

    public bool Commit(IChangeReportBuilder<TVerifier, THighlighter> builder)
    {
        return NotEmpty();
    }

    public void Push(ICommitMaker pusher)
    {
        foreach (var solution in _solutionAddedBuffer)
        {
            _producer.ExecuteChange(new SolverProgress(ProgressType.SolutionAddition, solution));
        }

        foreach (var possibility in _possibilityRemovedBuffer)
        {
            _producer.ExecuteChange(new SolverProgress(ProgressType.PossibilityRemoval, possibility));
        }
        
        _solutionAddedBuffer.Clear();
        _possibilityRemovedBuffer.Clear();
    }

    public void PushCommit(BuiltChangeCommit<THighlighter> commit)
    {
        foreach (var change in commit.Changes)
        {
            _producer.ExecuteChange(change);
        }
    }
}