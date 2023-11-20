using System.Collections.Generic;
using System.Linq;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.LinkGraph;
using Model.Solver.StrategiesUtility.NRCZTChains;

namespace Model.Solver.Strategies.NRCZTChains;

public class ZCondition : INRCZTCondition
{
    public string Name => "Z";

    public IEnumerable<(CellPossibility, INRCZTConditionChainManipulation)> SearchEndUnderCondition(
        IStrategyManager strategyManager, LinkGraph<CellPossibility> graph, BlockChain chain,
        CellPossibility bStart)
    {
        var all = chain.AllCellPossibilities();
        
        var possibilities = strategyManager.PossibilitiesAt(bStart.Row, bStart.Col);
        if (possibilities.Count == 3)
        {
            bool ok = true;
            
            foreach (var p in possibilities)
            {
                if (all.Contains(new CellPossibility(bStart.Row, bStart.Col, p)))
                {
                    ok = false;
                    break;
                }
            }

            if (ok)
            {
                
            }
        }
        
        yield break;
    }
}