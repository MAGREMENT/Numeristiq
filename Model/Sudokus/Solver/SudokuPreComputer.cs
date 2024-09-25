using System.Collections.Generic;
using Model.Core.Graphs;
using Model.Core.Graphs.Coloring;
using Model.Core.Graphs.Coloring.ColoringResults;
using Model.Core.Graphs.Implementations;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility.AlmostLockedSets;
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

    private bool _wasPreColorUsed;
    private readonly ColoringDictionary<ISudokuElement>?[,,] _onColoring
        = new ColoringDictionary<ISudokuElement>[9, 9, 9];

    private List<JuniorExocet>? _jes;
    
    private DoubleIntArgumentPrecomputing _oddagonPrecomputing = DoubleIntArgumentPrecomputing.NotDone;
    private List<AlmostOddagon> _oddagons = new();
    
    private DoubleIntArgumentPrecomputing _alsPrecomputing = DoubleIntArgumentPrecomputing.NotDone;
    private DoubleIntArgumentPrecomputing _alsGraphPrecomputing = DoubleIntArgumentPrecomputing.NotDone;
    private DoubleIntArgumentPrecomputing _ahsGraphPrecomputing = DoubleIntArgumentPrecomputing.NotDone;
    private List<IPossibilitySet> _als = new();
    private IGraph<IPossibilitySet, ReadOnlyBitSet16> _alsGraph = new HDictionaryGraph<IPossibilitySet, ReadOnlyBitSet16>();
    private IGraph<IPossibilitySet, Cell[]> _ahsGraph = new HDictionaryGraph<IPossibilitySet, Cell[]>();
    
    public ConstructedGraph<ISudokuSolverData, IGraph<CellPossibility, LinkStrength>> SimpleGraph { get; }
    public ConstructedGraph<ISudokuSolverData, IConditionalGraph<ISudokuElement, LinkStrength, ElementColor>> ComplexGraph { get; }

    public SudokuPreComputer(ISudokuSolverData solverData)
    {
        _solverData = solverData;
        SimpleGraph = new ConstructedGraph<ISudokuSolverData, IGraph<CellPossibility, LinkStrength>>(
            new HDictionaryLinkGraph<CellPossibility>(), _solverData);
        ComplexGraph = new ConstructedGraph<ISudokuSolverData, IConditionalGraph<ISudokuElement, LinkStrength, ElementColor>>(
            new HDictionaryConditionalLinkGraph<ISudokuElement, ElementColor>(), _solverData);
    }

    public void Reset()
    {
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
        
        _oddagonPrecomputing = DoubleIntArgumentPrecomputing.NotDone;
        _oddagons.Clear();
        
        _alsPrecomputing = DoubleIntArgumentPrecomputing.NotDone;
        _alsGraphPrecomputing = DoubleIntArgumentPrecomputing.NotDone;
        _ahsGraphPrecomputing = DoubleIntArgumentPrecomputing.NotDone;
        _als.Clear();
        _alsGraph.Clear();
        _ahsGraph.Clear();
        
        SimpleGraph.Clear();
        ComplexGraph.Clear();
    }

    public ColoringDictionary<ISudokuElement> OnColoring(int row, int col, int possibility)
    {
        _wasPreColorUsed = true;

        _onColoring[row, col, possibility - 1] ??=
            DoColor(new CellPossibility(row, col, possibility), ElementColor.On);
        return _onColoring[row, col, possibility - 1]!;
    }

    public ColoringDictionary<ISudokuElement> OffColoring(int row, int col, int possibility)
    {
        return DoColor(new CellPossibility(row, col, possibility), ElementColor.Off);
    }

    public List<JuniorExocet> JuniorExocet()
    {
        _jes ??= DoJuniorExocet();
        return _jes;
    }

    public List<AlmostOddagon> AlmostOddagons(int maxLength, int maxGuardians)
    {
        if (_oddagonPrecomputing.Corresponds(maxLength, maxGuardians)) return _oddagons;
        
        _oddagons = DoAlmostOddagons(maxLength, maxGuardians);
        _oddagonPrecomputing = new DoubleIntArgumentPrecomputing(true, maxLength, maxGuardians);
        return _oddagons;
    }
    
    public List<IPossibilitySet> AlmostLockedSets(int maxSize)
    {
        if (_alsPrecomputing.Corresponds(maxSize, 1)) return _als;
        
        _als = DoAlmostLockedSets(maxSize);
        _alsPrecomputing = new DoubleIntArgumentPrecomputing(true, maxSize, 1);
        return _als;
    }

    public IGraph<IPossibilitySet, ReadOnlyBitSet16> AlmostLockedSetGraph(int maxSize)
    {
        if (_alsGraphPrecomputing.Corresponds(maxSize, 1)) return _alsGraph;
        
        _alsGraph = DoAlmostLockedSetGraph(maxSize);
        _alsGraphPrecomputing = new DoubleIntArgumentPrecomputing(true, maxSize, 1);
        return _alsGraph;
    }

    public IEnumerable<LinkedAlmostLockedSets> ConstructAlmostLockedSetGraph(int maxSize)
    {
        _alsGraph = new HDictionaryGraph<IPossibilitySet, ReadOnlyBitSet16>();
        _alsGraphPrecomputing = new DoubleIntArgumentPrecomputing(true, maxSize, 1);
        return DoAlmostLockedSetGraph(_alsGraph, maxSize);
    }

    public IGraph<IPossibilitySet, Cell[]> AlmostHiddenSetGraph(int maxSize)
    {
        if (_ahsGraphPrecomputing.Corresponds(maxSize, 1)) return _ahsGraph;
        
        _ahsGraph = DoAlmostHiddenSetGraph(maxSize);
        _ahsGraphPrecomputing = new DoubleIntArgumentPrecomputing(true, maxSize, 1);
        return _ahsGraph;
    }

    public IEnumerable<LinkedAlmostHiddenSets> ConstructAlmostHiddenSetGraph(int maxSize)
    {
        _ahsGraph = new HDictionaryGraph<IPossibilitySet, Cell[]>();
        _ahsGraphPrecomputing = new DoubleIntArgumentPrecomputing(true, maxSize, 1);
        return DoAlmostHiddenSetGraph(_ahsGraph, maxSize);
    }
    
    //Private-----------------------------------------------------------------------------------------------------------

    private ColoringDictionary<ISudokuElement> DoColor(ISudokuElement start, ElementColor firstColor)
    {
        ComplexGraph.Construct(CellStrongLinkConstructionRule.Instance, CellWeakLinkConstructionRule.Instance,
            UnitStrongLinkConstructionRule.Instance, UnitWeakLinkConstructionRule.Instance,
            PointingPossibilitiesConstructionRule.Instance, AlmostNakedSetConstructionRule.Instance,
            ConditionalStrongLinkConstructionRule.Instance);

        return ColorHelper.ColorFromStart<ISudokuElement, ColoringDictionary<ISudokuElement>>(
            ColorHelper.Algorithm.ColorConditionalWithRules, ComplexGraph.Graph, start, firstColor, true);
    }

    private List<JuniorExocet> DoJuniorExocet()
    {
        return ExocetSearcher.SearchJuniors(_solverData);
    }

    private List<AlmostOddagon> DoAlmostOddagons(int maxLength, int maxGuardians)
    {
        return OddagonSearcher.Search(_solverData, maxLength, maxGuardians);
    }
    
    private List<IPossibilitySet> DoAlmostLockedSets(int maxSize)
    {
        return AlmostNakedSetSearcher.FullGrid(_solverData, maxSize, 1);
    }

    private IGraph<IPossibilitySet, ReadOnlyBitSet16> DoAlmostLockedSetGraph(int maxSize)
    {
        var graph = new HDictionaryGraph<IPossibilitySet, ReadOnlyBitSet16>();
        var allAls = AlmostLockedSets(maxSize);

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
        IGraph<IPossibilitySet, ReadOnlyBitSet16> graph, int maxSize)
    {
        var allAls = AlmostLockedSets(maxSize);

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

    private IGraph<IPossibilitySet, Cell[]> DoAlmostHiddenSetGraph(int maxSize)
    {
        var graph = new HDictionaryGraph<IPossibilitySet, Cell[]>();
        var allAhs = AlmostHiddenSetSearcher.FullGrid(_solverData, maxSize, 1);

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
    
    private IEnumerable<LinkedAlmostHiddenSets> DoAlmostHiddenSetGraph(IGraph<IPossibilitySet, Cell[]> graph,
        int maxSize)
    {
        var allAhs = AlmostHiddenSetSearcher.FullGrid(_solverData, maxSize, 1);

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

    private readonly struct DoubleIntArgumentPrecomputing
    {
        private bool _done { get; }
        private int _arg1 { get; }
        private int _arg2 { get; }
        
        public DoubleIntArgumentPrecomputing(bool done, int arg1, int arg2)
        {
            _done = done;
            _arg1 = arg1;
            _arg2 = arg2;
        }

        public bool Corresponds(int arg1, int arg2)
        {
            return _done && arg1 == _arg1 && arg2 == _arg2;
        }

        public static DoubleIntArgumentPrecomputing NotDone => new(false, 0, 0);
    }
}

public record LinkedAlmostLockedSets(IPossibilitySet One, IPossibilitySet Two, ReadOnlyBitSet16 RestrictedCommons);
public record LinkedAlmostHiddenSets(IPossibilitySet One, IPossibilitySet Two, Cell[] Cells);