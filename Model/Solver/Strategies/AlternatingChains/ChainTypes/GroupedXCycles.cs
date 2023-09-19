using System.Collections.Generic;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies.AlternatingChains.ChainTypes;

public class GroupedXCycles : IAlternatingChainType<ILinkGraphElement>
{
    public string Name => "XCycles";
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public IStrategy? Strategy { get; set; }
    
    public LinkGraph<ILinkGraphElement> GetGraph(IStrategyManager view)
    {
        view.GraphManager.Construct(ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink,
            ConstructRule.PointingPossibilities);
        return view.GraphManager.LinkGraph;
    }

    public bool ProcessFullLoop(IStrategyManager view, Loop<ILinkGraphElement> loop)
    {
        loop.ForEachLink((one, two)
            => ProcessWeakLink(view, one, two), LinkStrength.Weak);

        return view.ChangeBuffer.Push(Strategy!, new AlternatingChainReportBuilder<ILinkGraphElement>(loop));
    }
    
    //TODO make this bullshit better for the eyes
    private void ProcessWeakLink(IStrategyManager view, ILinkGraphElement one, ILinkGraphElement two)
    {
        switch (one)
        {
            case PointingRow rOne when two is PointingRow rTwo :
                RemovePossibilityInAll(view, rOne.SharedSeenCells(rTwo));
                break;
            case PointingRow rOne when two is CellPossibility sTwo :
                RemovePossibilityInAll(view, rOne.SharedSeenCells(sTwo));
                break;
            case CellPossibility sOne when two is PointingRow rTwo :
                RemovePossibilityInAll(view, rTwo.SharedSeenCells(sOne));
                break;
            case CellPossibility sOne when two is CellPossibility sTwo :
                RemovePossibilityInAll(view, sOne.SharedSeenCells(sTwo), sOne.Possibility);
                break;
            case CellPossibility sOne when two is PointingColumn cTwo :
                RemovePossibilityInAll(view, cTwo.SharedSeenCells(sOne));
                break;
            case PointingColumn cOne when two is PointingColumn cTwo :
                RemovePossibilityInAll(view, cOne.SharedSeenCells(cTwo));
                break;
            case PointingColumn cOne when two is CellPossibility sTwo :
                RemovePossibilityInAll(view, cOne.SharedSeenCells(sTwo));
                break;
        }
    }

    private void RemovePossibilityInAll(IStrategyManager view, IEnumerable<CellPossibility> coords)
    {
        foreach (var coord in coords)
        {
            view.ChangeBuffer.AddPossibilityToRemove(coord.Possibility, coord.Row, coord.Col);
        }
    }
    
    private void RemovePossibilityInAll(IStrategyManager view, IEnumerable<Cell> coords, int possibility)
    {
        foreach (var coord in coords)
        {
            view.ChangeBuffer.AddPossibilityToRemove(possibility, coord.Row, coord.Col);
        }
    }

    public bool ProcessWeakInference(IStrategyManager view, ILinkGraphElement inference, Loop<ILinkGraphElement> loop)
    {
        if (inference is not CellPossibility single) return false;
        view.ChangeBuffer.AddPossibilityToRemove(single.Possibility, single.Row, single.Col);

        return view.ChangeBuffer.Push(Strategy!, new AlternatingChainReportBuilder<ILinkGraphElement>(loop));
    }

    public bool ProcessStrongInference(IStrategyManager view, ILinkGraphElement inference, Loop<ILinkGraphElement> loop)
    {
        if (inference is not CellPossibility single) return false;
        view.ChangeBuffer.AddDefinitiveToAdd(single.Possibility, single.Row, single.Col);
        
        return view.ChangeBuffer.Push(Strategy!, new AlternatingChainReportBuilder<ILinkGraphElement>(loop));
    }
}