using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.PossibilityPosition;
using Model.Sudokus.Solver.StrategiesUtility;
using Model.Sudokus.Solver.StrategiesUtility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class AlmostLockedSetsChainStrategy : SudokuStrategy
{
    public const string OfficialName = "Almost Locked Sets Chain";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly bool _checkLength2;

    public AlmostLockedSetsChainStrategy(bool checkLength2) : base(OfficialName, StrategyDifficulty.Extreme, DefaultInstanceHandling)
    {
        _checkLength2 = checkLength2;
    }
    
    public override void Apply(IStrategyUser strategyUser)
    {
        var graph = strategyUser.PreComputer.AlmostLockedSetGraph();

        foreach (var start in graph)
        {
            if(Search(strategyUser, graph, start.Positions, new HashSet<IPossibilitiesPositions> {start},
                   new ChainBuilder<IPossibilitiesPositions, int>(start))) return;
        }
    }

    private bool Search(IStrategyUser strategyUser, PossibilitiesGraph<IPossibilitiesPositions> graph,
        GridPositions occupied, HashSet<IPossibilitiesPositions> explored, ChainBuilder<IPossibilitiesPositions, int> chain)
    {
        foreach (var friend in graph.GetLinks(chain.LastElement()))
        {
            /*if (chain.Count > 2 && chain.FirstElement().Equals(friend.To) &&
                CheckForLoop(strategyManager, chain, friend.Possibilities, occupied)) return true;*/
            
            if (explored.Contains(friend.To) || occupied.ContainsAny(friend.To.Positions)) continue;

            var lastLink = chain.LastLink();
            foreach (var possibleLink in friend.Possibilities.EnumeratePossibilities())
            {
                if (lastLink == possibleLink) continue;

                chain.Add(possibleLink, friend.To);
                explored.Add(friend.To);
                var occupiedCopy = occupied.Or(friend.To.Positions);

                if (CheckForChain(strategyUser, chain)) return true;
                if (Search(strategyUser, graph, occupiedCopy, explored, chain)) return true;
                
                chain.RemoveLast();
            }
        }
        
        return false;
    }

    private bool CheckForLoop(IStrategyUser strategyUser, ChainBuilder<IPossibilitiesPositions, int> builder,
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
                    cells.AddRange(element.EachCell());
                    
                    var linkAfter = i == chain.Elements.Length - 1 ? ll : chain.Links[i];
                    if (p == linkAfter) cells.AddRange(i == chain.Elements.Length - 1 
                        ? chain.Elements[0].EachCell() : chain.Elements[i + 1].EachCell());

                    foreach (var ssc in cells)
                    {
                        if (occupied.Contains(ssc)) continue;

                        strategyUser.ChangeBuffer.ProposePossibilityRemoval(p, ssc);
                    }
                }
            }

            return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                new AlmostLockedSetsChainReportBuilder(chain, ll)) && StopOnFirstPush;
        }
        
        return false;
    }

    private bool CheckForChain(IStrategyUser strategyUser, ChainBuilder<IPossibilitiesPositions, int> chain)
    {
        if (!_checkLength2 && chain.Count == 2) return false;

        var first = chain.FirstElement();
        var last = chain.LastElement();

        var nope = new ReadOnlyBitSet16(chain.FirstLink(), chain.LastLink());

        foreach (var possibility in first.Possibilities.EnumeratePossibilities())
        {
            if (!last.Possibilities.Contains(possibility) || nope.Contains(possibility)) continue;

            var cells = new List<Cell>();
            cells.AddRange(first.EachCell(possibility));
            cells.AddRange(last.EachCell(possibility));

            foreach (var ssc in SudokuCellUtility.SharedSeenCells(cells))
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, ssc);
            }
        }

        return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
            new AlmostLockedSetsChainReportBuilder(chain.ToChain())) && StopOnFirstPush;
    }
}

public class AlmostLockedSetsChainReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly Chain<IPossibilitiesPositions, int> _chain;
    private readonly int _possibleLastLink;

    public AlmostLockedSetsChainReportBuilder(Chain<IPossibilitiesPositions, int> chain)
    {
        _chain = chain;
        _possibleLastLink = -1;
    }
    
    public AlmostLockedSetsChainReportBuilder(Chain<IPossibilitiesPositions, int> chain, int lastLink)
    {
        _chain = chain;
        _possibleLastLink = lastLink;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( _chain.ToString(), lighter =>
        {
            var color = (int)ChangeColoration.CauseOffOne;
            foreach (var als in _chain.Elements)
            {
                foreach (var cell in als.EachCell())
                {
                    lighter.HighlightCell(cell, (ChangeColoration)color);
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

    private void HighlightLink(ISudokuHighlighter lighter, int link, IPossibilitiesPositions elementBefore, IPossibilitiesPositions elementAfter)
    {
        foreach (var cell in elementBefore.EachCell(link))
        {
            lighter.HighlightPossibility(link, cell.Row, cell.Column, ChangeColoration.Neutral);
        }
                
        foreach (var cell in elementAfter.EachCell(link))
        {
            lighter.HighlightPossibility(link, cell.Row, cell.Column, ChangeColoration.Neutral);
        }

        var minDistance = double.MaxValue;
        var minCells = new CellPossibility[2];
                
        foreach (var cell1 in elementBefore.EachCell(link))
        {
            foreach (var cell2 in elementAfter.EachCell(link))
            {
                var dist = SudokuCellUtility.Distance(cell1, link, cell2, link);

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
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}