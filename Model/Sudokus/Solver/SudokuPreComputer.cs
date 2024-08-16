using System.Collections.Generic;
using Model.Core.Graphs;
using Model.Core.Graphs.Implementations;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility.CellColoring;
using Model.Sudokus.Solver.Utility.CellColoring.ColoringResults;
using Model.Sudokus.Solver.Utility.Exocet;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Sudokus.Solver.Utility.Graphs.ConstructRules;
using Model.Sudokus.Solver.Utility.Oddagons;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver;

public class SudokuPreComputer
{
    private readonly ISudokuSolverData _solverData;

    private List<IPossibilitySet>? _als;

    private readonly ColoringDictionary<ISudokuElement>?[,,] _onColoring
        = new ColoringDictionary<ISudokuElement>[9, 9, 9];

    private bool _wasPreColorUsed;

    private List<JuniorExocet>? _jes;
    private List<AlmostOddagon>? _oddagons;

    private IGraph<IPossibilitySet, ReadOnlyBitSet16>? _alsGraph;
    private IGraph<IPossibilitySet, Cell[]>? _ahsGraph;
    
    public ManagedLinkGraph<ISudokuSolverData, CellPossibility> SimpleGraph { get; }
    public ManagedLinkGraph<ISudokuSolverData, ISudokuElement> ComplexGraph { get; }

    public SudokuPreComputer(ISudokuSolverData solverData)
    {
        _solverData = solverData;
        SimpleGraph = new ManagedLinkGraph<ISudokuSolverData, CellPossibility>(
            new HDictionaryLinkGraph<CellPossibility>(), _solverData);
        ComplexGraph = new ManagedLinkGraph<ISudokuSolverData, ISudokuElement>(
            new HDictionaryLinkGraph<ISudokuElement>(), _solverData);
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
        _oddagons = null;
        _alsGraph = null;
        _alsGraph = null;
        
        SimpleGraph.Clear();
        ComplexGraph.Clear();
    }

    public List<IPossibilitySet> AlmostLockedSets()
    {
        _als ??= DoAlmostLockedSets();
        return _als;
    }

    public ColoringDictionary<ISudokuElement> OnColoring(int row, int col, int possibility)
    {
        _wasPreColorUsed = true;

        _onColoring[row, col, possibility - 1] ??=
            DoColor(new CellPossibility(row, col, possibility), Coloring.On);
        return _onColoring[row, col, possibility - 1]!;
    }

    public ColoringDictionary<ISudokuElement> OffColoring(int row, int col, int possibility)
    {
        return DoColor(new CellPossibility(row, col, possibility), Coloring.Off);
    }

    public List<JuniorExocet> JuniorExocet()
    {
        _jes ??= DoJuniorExocet();
        return _jes;
    }

    public List<AlmostOddagon> AlmostOddagons()
    {
        _oddagons ??= DoAlmostOddagons();
        return _oddagons;
    }

    public IGraph<IPossibilitySet, ReadOnlyBitSet16> AlmostLockedSetGraph()
    {
        _alsGraph ??= DoAlmostLockedSetGraph();
        return _alsGraph;
    }

    public IEnumerable<LinkedAlmostLockedSets> ConstructAlmostLockedSetGraph()
    {
        _alsGraph = new HDictionaryGraph<IPossibilitySet, ReadOnlyBitSet16>();
        return DoAlmostLockedSetGraph(_alsGraph);
    }

    public IGraph<IPossibilitySet, Cell[]> AlmostHiddenSetGraph()
    {
        _ahsGraph ??= DoAlmostHiddenSetGraph();
        return _ahsGraph;
    }

    public IEnumerable<LinkedAlmostHiddenSets> ConstructAlmostHiddenSetGraph()
    {
        _ahsGraph = new HDictionaryGraph<IPossibilitySet, Cell[]>();
        return DoAlmostHiddenSetGraph(_ahsGraph);
    }
    
    //Private-----------------------------------------------------------------------------------------------------------

    private List<IPossibilitySet> DoAlmostLockedSets()
    {
        return _solverData.AlmostNakedSetSearcher.FullGrid(5, 1);
    }

    private ColoringDictionary<ISudokuElement> DoColor(ISudokuElement start, Coloring firstColor)
    {
        ComplexGraph.Construct(CellStrongLinkConstructRule.Instance, CellWeakLinkConstructRule.Instance,
            UnitStrongLinkConstructRule.Instance, UnitWeakLinkConstructRule.Instance,
            PointingPossibilitiesConstructRule.Instance, AlmostNakedSetConstructRule.Instance);

        return ColorHelper.ColorFromStart<ISudokuElement, ColoringDictionary<ISudokuElement>>(
            ColorHelper.Algorithm.ColorWithRulesAndLinksJump, ComplexGraph.Graph, start, firstColor, true);
    }

    private List<JuniorExocet> DoJuniorExocet()
    {
        return ExocetSearcher.SearchJuniors(_solverData);
    }

    private List<AlmostOddagon> DoAlmostOddagons()
    {
        return OddagonSearcher.Search(_solverData, 7, 3);
    }

    private IGraph<IPossibilitySet, ReadOnlyBitSet16> DoAlmostLockedSetGraph()
    {
        var graph = new HDictionaryGraph<IPossibilitySet, ReadOnlyBitSet16>();
        var allAls = AlmostLockedSets();

        for (int i = 0; i < allAls.Count; i++)
        {
            for (int j = i + 1; j < allAls.Count; j++)
            {
                var one = allAls[i];
                var two = allAls[j];
                if (one.Positions.ContainsAny(two.Positions)) continue;

                var rcc = one.RestrictedCommons(two);
                if(rcc.Count > 0) graph.Add(one, two, rcc);
            }
        }

        return graph;
    }

    private IEnumerable<LinkedAlmostLockedSets> DoAlmostLockedSetGraph(
        IGraph<IPossibilitySet, ReadOnlyBitSet16> graph)
    {
        var allAls = AlmostLockedSets();

        for (int i = 0; i < allAls.Count; i++)
        {
            for (int j = i + 1; j < allAls.Count; j++)
            {
                var one = allAls[i];
                var two = allAls[j];
                if (one.Positions.ContainsAny(two.Positions)) continue;

                var rcc = one.RestrictedCommons(two);
                if (rcc.Count == 0) continue;
                
                graph.Add(one, two, rcc);
                yield return new LinkedAlmostLockedSets(one, two, rcc);
            }
        }
    }

    private IGraph<IPossibilitySet, Cell[]> DoAlmostHiddenSetGraph()
    {
        var graph = new HDictionaryGraph<IPossibilitySet, Cell[]>();
        var allAhs = _solverData.AlmostHiddenSetSearcher.FullGrid(5, 1);

        for (int i = 0; i < allAhs.Count; i++)
        {
            for (int j = i + 1; j < allAhs.Count; j++)
            {
                var one = allAhs[i];
                var two = allAhs[j];

                if (one.Possibilities.ContainsAny(two.Possibilities)) continue;

                var and = one.Positions.And(two.Positions);
                if(and.Count > 0) graph.Add(one, two, and.ToArray());
            }
        }

        return graph;
    }
    
    private IEnumerable<LinkedAlmostHiddenSets> DoAlmostHiddenSetGraph(IGraph<IPossibilitySet, Cell[]> graph)
    {
        var allAhs = _solverData.AlmostHiddenSetSearcher.FullGrid(5, 1);

        for (int i = 0; i < allAhs.Count; i++)
        {
            for (int j = i + 1; j < allAhs.Count; j++)
            {
                var one = allAhs[i];
                var two = allAhs[j];

                if (one.Possibilities.ContainsAny(two.Possibilities)) continue;

                var and = one.Positions.And(two.Positions);
                if (and.Count == 0) continue;

                var asArray = and.ToArray();
                graph.Add(one, two, asArray);

                yield return new LinkedAlmostHiddenSets(one, two, asArray);
            }
        }
    }
}

public record LinkedAlmostLockedSets(IPossibilitySet One, IPossibilitySet Two, ReadOnlyBitSet16 RestrictedCommons);
public record LinkedAlmostHiddenSets(IPossibilitySet One, IPossibilitySet Two, Cell[] Cells);