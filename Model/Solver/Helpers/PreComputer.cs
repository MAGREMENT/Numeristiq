using System.Collections.Generic;
using Global;
using Model.Solver.Possibility;
using Model.Solver.PossibilityPosition;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.CellColoring;
using Model.Solver.StrategiesUtility.CellColoring.ColoringResults;
using Model.Solver.StrategiesUtility.Exocet;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Helpers;

public class PreComputer
{
    private readonly IStrategyManager _strategyManager;

    private List<IPossibilitiesPositions>? _als;

    private readonly ColoringDictionary<ILinkGraphElement>?[,,] _onColoring
        = new ColoringDictionary<ILinkGraphElement>[9, 9, 9];
    private bool _wasPreColorUsed;

    private List<JuniorExocet>? _jes;

    private PossibilitiesGraph<IPossibilitiesPositions>? _alsGraph;
    private PositionsGraph<IPossibilitiesPositions>? _ahsGraph;

    public PreComputer(IStrategyManager strategyManager)
    {
        _strategyManager = strategyManager;
    }

    public void Reset()
    {
        _als = null;
        
        if (_wasPreColorUsed)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        _onColoring[i, j, k] = null;
                    }
                }
            }
            _wasPreColorUsed = false;
        }

        _jes = null;
        _alsGraph = null;
        _alsGraph = null;
    }

    public List<IPossibilitiesPositions> AlmostLockedSets()
    {
        _als ??= DoAlmostLockedSets();
        return _als;
    }

    public ColoringDictionary<ILinkGraphElement> OnColoring(int row, int col, int possibility)
    {
        _wasPreColorUsed = true;

        _onColoring[row, col, possibility - 1] ??=
            DoColor(new CellPossibility(row, col, possibility), Coloring.On);
        return _onColoring[row, col, possibility - 1]!;
    }
    
    public ColoringDictionary<ILinkGraphElement> OffColoring(int row, int col, int possibility)
    {
        return DoColor(new CellPossibility(row, col, possibility), Coloring.Off);
    }

    public List<JuniorExocet> JuniorExocet()
    {
        _jes ??= DoJuniorExocet();
        return _jes;
    }

    public PossibilitiesGraph<IPossibilitiesPositions> AlmostLockedSetGraph()
    {
        _alsGraph ??= DoAlmostLockedSetGraph();
        return _alsGraph;
    }

    public IEnumerable<LinkedAlmostLockedSets> ConstructAlmostLockedSetGraph()
    {
        _alsGraph = new PossibilitiesGraph<IPossibilitiesPositions>();
        return DoAlmostLockedSetGraph(_alsGraph);
    }

    public PositionsGraph<IPossibilitiesPositions> AlmostHiddenSetGraph()
    {
        _ahsGraph ??= DoAlmostHiddenSetGraph();
        return _ahsGraph;
    }

    public IEnumerable<LinkedAlmostHiddenSets> ConstructAlmostHiddenSetGraph()
    {
        _ahsGraph = new PositionsGraph<IPossibilitiesPositions>();
        return DoAlmostHiddenSetGraph(_ahsGraph);
    }
    
    //Private-----------------------------------------------------------------------------------------------------------

    private List<IPossibilitiesPositions> DoAlmostLockedSets()
    {
        return _strategyManager.AlmostNakedSetSearcher.FullGrid();
    }

    private ColoringDictionary<ILinkGraphElement> DoColor(ILinkGraphElement start, Coloring firstColor)
    {
        _strategyManager.GraphManager.ConstructComplex(ConstructRule.CellStrongLink, ConstructRule.CellWeakLink,
            ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink, ConstructRule.PointingPossibilities,
            ConstructRule.AlmostNakedPossibilities, ConstructRule.JuniorExocet);
        var graph = _strategyManager.GraphManager.ComplexLinkGraph;

        return ColorHelper.ColorFromStart<ILinkGraphElement, ColoringDictionary<ILinkGraphElement>>(
            ColorHelper.Algorithm.ColorWithRulesAndLinksJump, graph, start, firstColor, true);
    }

    private List<JuniorExocet> DoJuniorExocet()
    {
        return JuniorExocetSearcher.FullGrid(_strategyManager);
    }

    private PossibilitiesGraph<IPossibilitiesPositions> DoAlmostLockedSetGraph()
    {
        var graph = new PossibilitiesGraph<IPossibilitiesPositions>();
        var allAls = AlmostLockedSets();

        for (int i = 0; i < allAls.Count; i++)
        {
            for (int j = i + 1; j < allAls.Count; j++)
            {
                var one = allAls[i];
                var two = allAls[j];
                if (one.Positions.PeakAny(two.Positions)) continue;

                var rcc = one.RestrictedCommons(two);
                if(rcc.Count > 0) graph.Add(one, two, rcc);
            }
        }

        return graph;
    }

    private IEnumerable<LinkedAlmostLockedSets> DoAlmostLockedSetGraph(
        PossibilitiesGraph<IPossibilitiesPositions> graph)
    {
        var allAls = AlmostLockedSets();

        for (int i = 0; i < allAls.Count; i++)
        {
            for (int j = i + 1; j < allAls.Count; j++)
            {
                var one = allAls[i];
                var two = allAls[j];
                if (one.Positions.PeakAny(two.Positions)) continue;

                var rcc = one.RestrictedCommons(two);
                if (rcc.Count == 0) continue;
                
                graph.Add(one, two, rcc);
                yield return new LinkedAlmostLockedSets(one, two, rcc);
            }
        }
    }

    private PositionsGraph<IPossibilitiesPositions> DoAlmostHiddenSetGraph()
    {
        var graph = new PositionsGraph<IPossibilitiesPositions>();
        var allAhs = _strategyManager.AlmostHiddenSetSearcher.FullGrid();

        for (int i = 0; i < allAhs.Count; i++)
        {
            for (int j = i + 1; j < allAhs.Count; j++)
            {
                var one = allAhs[i];
                var two = allAhs[j];

                if (one.Possibilities.PeekAny(two.Possibilities)) continue;

                var and = one.Positions.And(two.Positions);
                if(and.Count > 0) graph.Add(one, two, and.ToArray());
            }
        }

        return graph;
    }
    
    private IEnumerable<LinkedAlmostHiddenSets> DoAlmostHiddenSetGraph(PositionsGraph<IPossibilitiesPositions> graph)
    {
        var allAhs = _strategyManager.AlmostHiddenSetSearcher.FullGrid();

        for (int i = 0; i < allAhs.Count; i++)
        {
            for (int j = i + 1; j < allAhs.Count; j++)
            {
                var one = allAhs[i];
                var two = allAhs[j];

                if (one.Possibilities.PeekAny(two.Possibilities)) continue;

                var and = one.Positions.And(two.Positions);
                if (and.Count == 0) continue;

                var asArray = and.ToArray();
                graph.Add(one, two, asArray);

                yield return new LinkedAlmostHiddenSets(one, two, asArray);
            }
        }
    }
}

public record LinkedAlmostLockedSets(IPossibilitiesPositions One, IPossibilitiesPositions Two, Possibilities RestrictedCommons);
public record LinkedAlmostHiddenSets(IPossibilitiesPositions One, IPossibilitiesPositions Two, Cell[] Cells);