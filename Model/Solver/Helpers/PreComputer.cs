using System.Collections.Generic;
using Model.Solver.PossibilityPosition;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.CellColoring;
using Model.Solver.StrategiesUtility.CellColoring.ColoringResults;
using Model.Solver.StrategiesUtility.Exocet;
using Model.Solver.StrategiesUtility.LinkGraph;

namespace Model.Solver.Helpers;

public class PreComputer
{
    private readonly IStrategyManager _strategyManager;

    private List<IPossibilitiesPositions>? _als;

    private readonly ColoringDictionary<ILinkGraphElement>?[,,] _onColoring
        = new ColoringDictionary<ILinkGraphElement>[9, 9, 9];
    private bool _wasPreColorUsed;

    private List<JuniorExocet>? _jes;

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
}