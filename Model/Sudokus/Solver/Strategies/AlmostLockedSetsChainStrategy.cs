using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class AlmostLockedSetsChainStrategy : SudokuStrategy
{
    public const string OfficialName = "Almost Locked Sets Chain";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly bool _checkLength2;

    public AlmostLockedSetsChainStrategy(bool checkLength2) : base(OfficialName, Difficulty.Extreme, DefaultInstanceHandling)
    {
        _checkLength2 = checkLength2;
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        var graph = solverData.PreComputer.AlmostLockedSetGraph();

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
            /*if (chain.Count > 2 && chain.FirstElement().Equals(friend.To) &&
                CheckForLoop(strategyManager, chain, friend.Possibilities, occupied)) return true;*/
            
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

    private bool CheckForLoop(ISudokuSolverData solverData, ChainBuilder<IPossibilitySet, int> builder,
        ReadOnlyBitSet16 possibleLastLinks, GridPositions occupied)
    {
        foreach (var ll in possibleLastLinks.EnumeratePossibilities())
        {
            if (ll.Equals(builder.FirstLink())) continue;

            var chain = builder.ToChain();
            for (int i = 0; i < chain.Elements.Length; i++)
            {
                var element = chain.Elements[i];
                foreach (var p in element.Possibilities.EnumeratePossibilities())
                {
                    var indexBefore = i - 1 < 0 ? chain.Links.Length - 1 : i - 1;
                    if (p == chain.Links[indexBefore]) continue;

                    var cells = new List<Cell>();
                    cells.AddRange(element.EnumerateCells());
                    
                    var linkAfter = i == chain.Elements.Length - 1 ? ll : chain.Links[i];
                    if (p == linkAfter) cells.AddRange(i == chain.Elements.Length - 1 
                        ? chain.Elements[0].EnumerateCells() : chain.Elements[i + 1].EnumerateCells());

                    foreach (var ssc in cells)
                    {
                        if (occupied.Contains(ssc)) continue;

                        solverData.ChangeBuffer.ProposePossibilityRemoval(p, ssc);
                    }
                }
            }

            if (solverData.ChangeBuffer.NeedCommit())
            {
                solverData.ChangeBuffer.Commit(new AlmostLockedSetsChainReportBuilder(chain, ll));
                if (StopOnFirstCommit) return true;
            }
        }
        
        return false;
    }

    private bool CheckForChain(ISudokuSolverData solverData, ChainBuilder<IPossibilitySet, int> chain)
    {
        if (!_checkLength2 && chain.Count == 2) return false;

        var first = chain.FirstElement();
        var last = chain.LastElement();

        var nope = new ReadOnlyBitSet16(chain.FirstLink(), chain.LastLink());

        foreach (var possibility in first.Possibilities.EnumeratePossibilities())
        {
            if (!last.Possibilities.Contains(possibility) || nope.Contains(possibility)) continue;

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
    private readonly int _possibleLastLink;

    public AlmostLockedSetsChainReportBuilder(Chain<IPossibilitySet, int> chain)
    {
        _chain = chain;
        _possibleLastLink = -1;
    }
    
    public AlmostLockedSetsChainReportBuilder(Chain<IPossibilitySet, int> chain, int lastLink)
    {
        _chain = chain;
        _possibleLastLink = lastLink;
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

            for (int i = 0; i < _chain.Links.Length; i++)
            {
                HighlightLink(lighter, _chain.Links[i], _chain.Elements[i], _chain.Elements[i + 1]);
            }
            
            if(_possibleLastLink != -1) HighlightLink(lighter, _possibleLastLink,
                _chain.Elements[^1], _chain.Elements[0]);

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

        var minDistance = double.MaxValue;
        var minCells = new CellPossibility[2];
                
        foreach (var cell1 in elementBefore.EnumerateCells(link))
        {
            foreach (var cell2 in elementAfter.EnumerateCells(link))
            {
                var dist = CellUtility.Distance(cell1, link, cell2, link);

                if (dist < minDistance)
                {
                    minDistance = dist;
                    minCells[0] = new CellPossibility(cell1, link);
                    minCells[1] = new CellPossibility(cell2, link);
                }
            }
        }
                
        lighter.CreateLink(minCells[0], minCells[1], LinkStrength.Strong);
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}