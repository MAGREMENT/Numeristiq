using System.Collections.Generic;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver.Utility;

namespace Model.Helpers.Changes.Buffers;

public class ClueGetterChangeBuffer<TVerifier, THighlighter> : IChangeBuffer<TVerifier, THighlighter> where TVerifier : IUpdatableSolvingState where THighlighter : ISolvingStateHighlighter
{
    private readonly HashSet<CellPossibility> _possibilityRemovedBuffer = new();
    private readonly HashSet<CellPossibility> _solutionAddedBuffer = new();
    
    private readonly List<ChangeCommit<TVerifier, THighlighter>> _commits = new();
    
    private readonly ICustomCommitComparer<TVerifier, THighlighter> _default = new DefaultCommitComparer<TVerifier, THighlighter>();

    private readonly IStepManagingChangeProducer<TVerifier, THighlighter> _producer;
    
    public Clue<THighlighter>? CurrentClue { get; private set; }

    public ClueGetterChangeBuffer(IStepManagingChangeProducer<TVerifier, THighlighter> producer)
    {
        _producer = producer;
    }
    
    public bool IsManagingSteps => false;

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
        if (_possibilityRemovedBuffer.Count == 0 && _solutionAddedBuffer.Count == 0) return false;
        
        _commits.Add(new ChangeCommit<TVerifier, THighlighter>(ChangeBufferHelper.EstablishChangeList(_solutionAddedBuffer, _possibilityRemovedBuffer), builder));
        return true;
    }

    public void Push(ICommitMaker pusher)
    {
        if (_commits.Count == 0)
        {
            CurrentClue = null;
            return;
        }
        
        var state = _producer.CurrentState;

        var best = _commits[0];
        var comparer = pusher as ICustomCommitComparer<TVerifier, THighlighter> ?? _default;

        for (int i = 1; i < _commits.Count; i++)
        {
            if (comparer.Compare(best, _commits[i]) < 0) best = _commits[i];
        }

        CurrentClue = best.Builder.BuildClue(best.Changes, state);
        _producer.FakeChange();
        
        _commits.Clear();
    }

    public void PushCommit(BuiltChangeCommit<THighlighter> commit)
    {
        
    }
}