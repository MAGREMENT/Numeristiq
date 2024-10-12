using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility.Graphs.ConstructRules;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class AlmostHiddenSetsChainStrategy : SudokuStrategy
{
    public const string OfficialName = "Almost Hidden Sets Chain";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly BooleanSetting _ignoreLength2;
    private readonly IntSetting _maxAhsSize;
    
    public AlmostHiddenSetsChainStrategy(bool ignoreLength2, int maxAhsSize) : base(OfficialName, Difficulty.Extreme, DefaultInstanceHandling)
    {
        _ignoreLength2 = new BooleanSetting("Ignore length two",
            "The algorithm will ignore chains with a length equal to 2", ignoreLength2);
        _maxAhsSize = new IntSetting("Max AHS Size", "The maximum size for the almost hidden sets",
            new SliderInteractionInterface(2, 5, 1), maxAhsSize);
    }

    public override IEnumerable<ISetting> EnumerateSettings()
    {
        yield return _ignoreLength2;
        yield return _maxAhsSize;
    }

    public override void Apply(ISudokuSolverData solverData)
    {
        var graph = solverData.PreComputer.AlmostHiddenSetGraph(_maxAhsSize.Value);
        solverData.PreComputer.SimpleGraph.Construct(UnitStrongLinkConstructionRule.Instance);
        var linkGraph = solverData.PreComputer.SimpleGraph.Graph;

        foreach (var start in graph)
        {
            if (Search(solverData, graph, start.EveryPossibilities(), new HashSet<IPossibilitySet> {start},
                    new ChainBuilder<IPossibilitySet, Cell>(start), linkGraph)) return;
        }
    }

    private bool Search(ISudokuSolverData solverData, IGraph<IPossibilitySet, Cell[]> graph,
        ReadOnlyBitSet16 occupied, HashSet<IPossibilitySet> explored, ChainBuilder<IPossibilitySet, Cell> chain,
        IGraph<CellPossibility, LinkStrength> linkGraph)
    {
        foreach (var friend in graph.NeighborsWithEdges(chain.LastElement()))
        {
            if (explored.Contains(friend.To) || occupied.ContainsAny(friend.To.EveryPossibilities())) continue;

            var lastLink = chain.LastLink();
            foreach (var possibleLink in friend.Edge)
            {
                if (chain.Count > 0 && lastLink == possibleLink) continue;

                chain.Add(possibleLink, friend.To);
                explored.Add(friend.To);
                var occupiedCopy = occupied | friend.To.EveryPossibilities();

                if (CheckForChain(solverData, chain, linkGraph)) return true;
                if(Search(solverData, graph, occupiedCopy, explored, chain, linkGraph)) return true;
                
                chain.RemoveLast();
            }
        }

        return false;
    }

    private bool CheckForChain(ISudokuSolverData solverData, ChainBuilder<IPossibilitySet, Cell> chain, IGraph<CellPossibility, LinkStrength> linkGraph)
    {
        if (_ignoreLength2.Value && chain.Count == 2) return false;
        
        List<Edge<CellPossibility>> links = new();

        var first = chain.FirstElement();
        var last = chain.LastElement();

        var nope = new GridPositions();
        nope.Add(chain.FirstLink());
        nope.Add(chain.LastLink());

        foreach (var cell in first.EnumerateCells())
        {
            if (nope.Contains(cell)) continue;
            
            foreach (var possibility in solverData.PossibilitiesAt(cell).EnumeratePossibilities())
            {
                if (first.Contains(possibility) || last.Contains(possibility)) continue;

                var current = new CellPossibility(cell, possibility);
                foreach (var friend in linkGraph.Neighbors(current, LinkStrength.Strong))
                {
                    if (nope.Contains(friend.ToCell())) continue;
                    
                    foreach (var friendOfFriend in linkGraph.Neighbors(friend, LinkStrength.Strong))
                    {
                        var asCell = friendOfFriend.ToCell();

                        if (last.Positions.Contains(friendOfFriend.ToCell()) && asCell != cell && !nope.Contains(asCell))
                        {
                            links.Add(new Edge<CellPossibility>(current, friend));
                            links.Add(new Edge<CellPossibility>(friend, friendOfFriend));

                            solverData.ChangeBuffer.ProposeSolutionAddition(friend);
                        }
                    }
                }
            }
        }
        
        if (solverData.ChangeBuffer.NeedCommit())
        {
            solverData.ChangeBuffer.Commit(new AlmostHiddenSetsChainReportBuilder(chain.ToChain(), links));
            if (StopOnFirstCommit) return true;
        }

        return false;
    }
}

public class AlmostHiddenSetsChainReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly Chain<IPossibilitySet, Cell> _chain;
    private readonly List<Edge<CellPossibility>> _links;

    public AlmostHiddenSetsChainReportBuilder(Chain<IPossibilitySet, Cell> chain, List<Edge<CellPossibility>> links)
    {
        _chain = chain;
        _links = links;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>("Almost Hidden Sets Chain : " +  _chain, lighter =>
        {
            var color = (int)StepColor.Cause1;
            foreach (var ahs in _chain.Elements)
            {
                foreach (var possibility in ahs.EnumeratePossibilities())
                {
                    foreach (var cell in ahs.EnumerateCells(possibility))
                    {
                        lighter.HighlightPossibility(possibility, cell.Row, cell.Column, (StepColor)color);
                    }
                }

                color++;
            }

            foreach (var cell in _chain.Links)
            {
                lighter.EncircleCell(cell);
            }

            foreach (var link in _links)
            {
                lighter.CreateLink(link.From, link.To, LinkStrength.Strong);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}