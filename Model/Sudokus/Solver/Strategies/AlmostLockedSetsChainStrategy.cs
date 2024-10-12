using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class AlmostLockedSetsChainStrategy : SudokuStrategy
{
    public const string OfficialName = "Almost Locked Sets Chain";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly BooleanSetting _ignoreLength2;
    private readonly IntSetting _maxAlsSize;

    public AlmostLockedSetsChainStrategy(bool ignoreLength2, int maxAlsSize) : base(OfficialName, Difficulty.Extreme, DefaultInstanceHandling)
    {
        _ignoreLength2 = new BooleanSetting("Ignore length two",
            "The algorithm will ignore chains with a length equal to 2", ignoreLength2);
        _maxAlsSize = new IntSetting("Max ALS Size", "The maximum size for the almost locked sets",
            new SliderInteractionInterface(2, 5, 1), maxAlsSize);
    }
    
    public override IEnumerable<ISetting> EnumerateSettings()
    {
        yield return _ignoreLength2;
        yield return _maxAlsSize;
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        var graph = solverData.PreComputer.AlmostLockedSetGraph(_maxAlsSize.Value);

        foreach (var start in graph)
        {
            if(Search(solverData, graph, start.Positions, new HashSet<IPossibilitySet> {start},
                   new ChainBuilder<IPossibilitySet, int>(start))) return;
        }
    }

    private bool Search(ISudokuSolverData solverData, IGraph<IPossibilitySet, ReadOnlyBitSet16> graph,
        GridPositions occupied, HashSet<IPossibilitySet> explored, ChainBuilder<IPossibilitySet, int> chain)
    {
        foreach (var friend in graph.NeighborsWithEdges(chain.LastElement()))
        {
            if (explored.Contains(friend.To) || occupied.ContainsAny(friend.To.Positions)) continue;

            var lastLink = chain.LastLink();
            foreach (var possibleLink in friend.Edge.EnumeratePossibilities())
            {
                if (lastLink == possibleLink) continue;

                chain.Add(possibleLink, friend.To);
                explored.Add(friend.To);
                var occupiedCopy = occupied.Or(friend.To.Positions);

                if (CheckForChain(solverData, chain)) return true;
                if (Search(solverData, graph, occupiedCopy, explored, chain)) return true;
                
                chain.RemoveLast();
            }
        }
        
        return false;
    }

    private bool CheckForChain(ISudokuSolverData solverData, ChainBuilder<IPossibilitySet, int> chain)
    {
        if (_ignoreLength2.Value && chain.Count == 2) return false;

        var first = chain.FirstElement();
        var last = chain.LastElement();

        var nope = new ReadOnlyBitSet16(chain.FirstLink(), chain.LastLink());

        foreach (var possibility in first.EnumeratePossibilities())
        {
            if (!last.Contains(possibility) || nope.Contains(possibility)) continue;

            var cells = new List<Cell>();
            cells.AddRange(first.EnumerateCells(possibility));
            cells.AddRange(last.EnumerateCells(possibility));

            foreach (var ssc in SudokuUtility.SharedSeenCells(cells))
            {
                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, ssc);
            }
        }

        if (solverData.ChangeBuffer.NeedCommit())
        {
            solverData.ChangeBuffer.Commit(new AlmostLockedSetsChainReportBuilder(chain.ToChain()));
            if (StopOnFirstCommit) return true;
        }

        return false;
    }
}

public class AlmostLockedSetsChainReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly Chain<IPossibilitySet, int> _chain;

    public AlmostLockedSetsChainReportBuilder(Chain<IPossibilitySet, int> chain)
    {
        _chain = chain;
    }
    
    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>("Almost Locked Sets Chain : " +  _chain, lighter =>
        {
            var color = (int)StepColor.Cause1;
            foreach (var als in _chain.Elements)
            {
                foreach (var cell in als.EnumerateCells())
                {
                    lighter.HighlightCell(cell, (StepColor)color);
                }

                color++;
            }

            for (int i = 0; i < _chain.Links.Count; i++)
            {
                HighlightLink(lighter, _chain.Links[i], _chain.Elements[i], _chain.Elements[i + 1]);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    private void HighlightLink(ISudokuHighlighter lighter, int link, IPossibilitySet elementBefore, IPossibilitySet elementAfter)
    {
        foreach (var cell in elementBefore.EnumerateCells(link))
        {
            lighter.HighlightPossibility(link, cell.Row, cell.Column, StepColor.Neutral);
        }
                
        foreach (var cell in elementAfter.EnumerateCells(link))
        {
            lighter.HighlightPossibility(link, cell.Row, cell.Column, StepColor.Neutral);
        }

        lighter.CreateLink(elementBefore, elementAfter, link);
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}