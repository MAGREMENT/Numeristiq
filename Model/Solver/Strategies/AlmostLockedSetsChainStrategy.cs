using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Position;
using Model.Solver.Possibility;
using Model.Solver.PossibilityPosition;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies;

public class AlmostLockedSetsChainStrategy : AbstractStrategy
{
    public const string OfficialName = "Almost Locked Sets Chain";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly bool _checkLength2;

    public AlmostLockedSetsChainStrategy(bool checkLength2) : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
        _checkLength2 = checkLength2;
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        var graph = strategyManager.PreComputer.AlmostLockedSetGraph();

        foreach (var start in graph)
        {
            if(Search(strategyManager, graph, start.Positions, new HashSet<IPossibilitiesPositions> {start},
                   new ChainBuilder<IPossibilitiesPositions, int>(start))) return;
        }
    }

    private bool Search(IStrategyManager strategyManager, PossibilitiesGraph<IPossibilitiesPositions> graph,
        GridPositions occupied, HashSet<IPossibilitiesPositions> explored, ChainBuilder<IPossibilitiesPositions, int> chain)
    {
        foreach (var friend in graph.GetLinks(chain.LastElement()))
        {
            /*if (chain.Count > 2 && chain.FirstElement().Equals(friend.To) &&
                CheckForLoop(strategyManager, chain, friend.Possibilities, occupied)) return true;*/
            
            if (explored.Contains(friend.To) || occupied.PeakAny(friend.To.Positions)) continue;

            var lastLink = chain.LastLink();
            foreach (var possibleLink in friend.Possibilities)
            {
                if (lastLink == possibleLink) continue;

                chain.Add(possibleLink, friend.To);
                explored.Add(friend.To);
                var occupiedCopy = occupied.Or(friend.To.Positions);

                if (CheckForChain(strategyManager, chain)) return true;
                if (Search(strategyManager, graph, occupiedCopy, explored, chain)) return true;
                
                chain.RemoveLast();
            }
        }
        
        return false;
    }

    private bool CheckForLoop(IStrategyManager strategyManager, ChainBuilder<IPossibilitiesPositions, int> builder,
        IReadOnlyPossibilities possibleLastLinks, GridPositions occupied)
    {
        foreach (var ll in possibleLastLinks)
        {
            if (ll.Equals(builder.FirstLink())) continue;

            var chain = builder.ToChain();
            for (int i = 0; i < chain.Elements.Length; i++)
            {
                var element = chain.Elements[i];
                foreach (var p in element.Possibilities)
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
                        if (occupied.Peek(ssc)) continue;

                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, ssc);
                    }
                }
            }

            return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                new AlmostLockedSetsChainReportBuilder(chain, ll)) && OnCommitBehavior == OnCommitBehavior.Return;
        }
        
        return false;
    }

    private bool CheckForChain(IStrategyManager strategyManager, ChainBuilder<IPossibilitiesPositions, int> chain)
    {
        if (!_checkLength2 && chain.Count == 2) return false;

        var first = chain.FirstElement();
        var last = chain.LastElement();

        var nope = Possibilities.NewEmpty();
        nope.Add(chain.FirstLink());
        nope.Add(chain.LastLink());

        foreach (var possibility in first.Possibilities)
        {
            if (!last.Possibilities.Peek(possibility) || nope.Peek(possibility)) continue;

            var cells = new List<Cell>();
            cells.AddRange(first.EachCell(possibility));
            cells.AddRange(last.EachCell(possibility));

            foreach (var ssc in Cells.SharedSeenCells(cells))
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, ssc);
            }
        }

        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
            new AlmostLockedSetsChainReportBuilder(chain.ToChain())) && OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public class AlmostLockedSetsChainReportBuilder : IChangeReportBuilder
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

    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), _chain.ToString(), lighter =>
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

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }

    private void HighlightLink(IHighlighter lighter, int link, IPossibilitiesPositions elementBefore, IPossibilitiesPositions elementAfter)
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
                var dist = Cells.Distance(cell1, link, cell2, link);

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
}