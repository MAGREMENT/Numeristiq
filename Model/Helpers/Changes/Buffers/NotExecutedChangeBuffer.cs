using System.Collections.Generic;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver.StrategiesUtility;

namespace Model.Helpers.Changes.Buffers;

public class NotExecutedChangeBuffer<TVerifier, THighlighter> : IChangeBuffer<TVerifier, THighlighter> where TVerifier : IUpdatableSolvingState where THighlighter : ISolvingStateHighlighter
{
    private readonly HashSet<CellPossibility> _possibilityRemovedBuffer = new();
    private readonly HashSet<CellPossibility> _solutionAddedBuffer = new();
        
    private readonly List<ChangeCommit<TVerifier, THighlighter>> _commitsBuffer = new();

    private readonly List<BuiltChangeCommit<THighlighter>> _commits = new();

    private readonly ILogManagedChangeProducer<TVerifier, THighlighter> _producer;

    public NotExecutedChangeBuffer(ILogManagedChangeProducer<TVerifier, THighlighter> producer)
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

    public bool Commit(IChangeReportBuilder<TVerifier, THighlighter> builder)
    {
        if (_possibilityRemovedBuffer.Count == 0 && _solutionAddedBuffer.Count == 0) return false;
        
        _commitsBuffer.Add(new ChangeCommit<TVerifier, THighlighter>(ChangeBufferHelper.EstablishChangeList(_solutionAddedBuffer, _possibilityRemovedBuffer), builder));
        return true;
    }

    public void Push(ICommitMaker pusher)
    {
        var snapshot = _producer.CurrentState;
        
        foreach (var commit in _commitsBuffer)
        {
            _commits.Add(new BuiltChangeCommit<THighlighter>(pusher, commit.Changes, commit.Builder.BuildReport(commit.Changes, snapshot)));
        }
        
        _commitsBuffer.Clear();
    }

    public void PushCommit(BuiltChangeCommit<THighlighter> commit)
    {
        
    }

    public BuiltChangeCommit<THighlighter>[] DumpCommits()
    {
        var result = _commits.ToArray();
        _commits.Clear();
        return result;
    }
}