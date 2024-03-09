using System.Collections.Generic;
using Model.Sudoku.Solver.StrategiesUtility;

namespace Model.Helpers.Changes.Buffers;

public class NotExecutedChangeBuffer : IChangeBuffer
{
    private readonly HashSet<CellPossibility> _possibilityRemovedBuffer = new();
    private readonly HashSet<CellPossibility> _solutionAddedBuffer = new();
        
    private readonly List<ChangeCommit> _commitsBuffer = new();

    private readonly List<BuiltChangeCommit> _commits = new();

    private readonly ILogManagedChangeProducer<IUpdatableSudokuSolvingState> _producer;

    public NotExecutedChangeBuffer(ILogManagedChangeProducer<IUpdatableSudokuSolvingState> producer)
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
        if(_producer.CanAddSolution(cp)) _solutionAddedBuffer.Add(cp);
    }

    public bool NotEmpty()
    {
        return _possibilityRemovedBuffer.Count > 0 || _solutionAddedBuffer.Count > 0;
    }

    public bool Commit(IChangeReportBuilder builder)
    {
        if (_possibilityRemovedBuffer.Count == 0 && _solutionAddedBuffer.Count == 0) return false;
        
        _commitsBuffer.Add(new ChangeCommit(ChangeBufferHelper.EstablishChangeList(_solutionAddedBuffer, _possibilityRemovedBuffer), builder));
        return true;
    }

    public void Push(ICommitMaker pusher)
    {
        var snapshot = _producer.CurrentState;
        
        foreach (var commit in _commitsBuffer)
        {
            _commits.Add(new BuiltChangeCommit(pusher, commit.Changes, commit.Builder.Build(commit.Changes, snapshot)));
        }
        
        _commitsBuffer.Clear();
    }

    public void PushCommit(BuiltChangeCommit commit)
    {
        
    }

    public BuiltChangeCommit[] DumpCommits()
    {
        var result = _commits.ToArray();
        _commits.Clear();
        return result;
    }
}