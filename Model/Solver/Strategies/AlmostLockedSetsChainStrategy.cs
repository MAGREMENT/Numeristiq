using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
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
            if (explored.Contains(friend.To) || occupied.PeakAny(friend.To.Positions)) continue;

            var lastLink = chain.LastLink();
            foreach (var possibleLink in friend.Possibilities)
            {
                if (lastLink == possibleLink) continue;

                chain.Add(possibleLink, friend.To);
                explored.Add(friend.To);
                var occupiedCopy = occupied.Or(friend.To.Positions);

                if (Check(strategyManager, chain)) return true;
                if (Search(strategyManager, graph, occupiedCopy, explored, chain)) return true;
                
                chain.RemoveLast();
            }
        }
        
        return false;
    }

    private bool Check(IStrategyManager strategyManager, ChainBuilder<IPossibilitiesPositions, int> chain)
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
            cells.AddRange(first.EachCell());
            cells.AddRange(last.EachCell());

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

    public AlmostLockedSetsChainReportBuilder(Chain<IPossibilitiesPositions, int> chain)
    {
        _chain = chain;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
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
                var poss = _chain.Links[i];

                foreach (var cell in _chain.Elements[i].EachCell(poss))
                {
                    lighter.HighlightPossibility(poss, cell.Row, cell.Col, ChangeColoration.Neutral);
                }
                
                foreach (var cell in _chain.Elements[i + 1].EachCell(poss))
                {
                    lighter.HighlightPossibility(poss, cell.Row, cell.Col, ChangeColoration.Neutral);
                }
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}