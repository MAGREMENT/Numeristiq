using System.Collections.Generic;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.CellColoring;
using Model.Solver.StrategiesUtil.CellColoring.ColoringResults;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Helpers;

public class PreComputer
{
    private readonly IStrategyManager _strategyManager;

    private List<AlmostLockedSet>? _als;

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

    public List<AlmostLockedSet> AlmostLockedSets()
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

    private List<AlmostLockedSet> DoAlmostLockedSets()
    {
        return AlmostLockedSetSearcher.FullGrid(_strategyManager);
    }

    private ColoringDictionary<ILinkGraphElement> DoColor(ILinkGraphElement start, Coloring firstColor)
    {
        _strategyManager.GraphManager.ConstructComplex(ConstructRule.CellStrongLink, ConstructRule.CellWeakLink,
            ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink, ConstructRule.PointingPossibilities,
            ConstructRule.AlmostNakedPossibilities, ConstructRule.JuniorExocet);
        var graph = _strategyManager.GraphManager.ComplexLinkGraph;

        return ColorHelper.ColorFromStart<ILinkGraphElement, ColoringDictionary<ILinkGraphElement>>(
            ColorHelper.Algorithm.ComplexColoring, graph, start, firstColor);
    }

    private List<JuniorExocet> DoJuniorExocet()
    {
        return StrategiesUtil.JuniorExocet.SearchFullGrid(_strategyManager);
    }
}