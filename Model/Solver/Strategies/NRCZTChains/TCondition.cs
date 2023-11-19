using System.Collections.Generic;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.LinkGraph;
using Model.Solver.StrategiesUtility.NRCZTChains;

namespace Model.Solver.Strategies.NRCZTChains;

public class TCondition : INRCZTCondition
{
    private readonly INRCZTConditionChainManipulation _manipulation = new EmptyChainManipulation();

    public string Name => "T";

    public IEnumerable<(CellPossibility, INRCZTConditionChainManipulation)> SearchEndUnderCondition(
        IStrategyManager strategyManager, LinkGraph<CellPossibility> graph, BlockChain chain, CellPossibility bStart)
    {
        var all = chain.AllCellPossibilities();
        
        var poss = strategyManager.PossibilitiesAt(bStart.Row, bStart.Col);
        if (poss.Count > 2)
        {
            var ignorable = Possibilities.NewEmpty();
            
            foreach (var p in poss)
            {
                if (p == bStart.Possibility) continue;

                var current = new CellPossibility(bStart.Row, bStart.Col, p);

                if (!all.Contains(current) && chain.IsWeaklyLinkedToAtLeastOneEnd(current))
                    ignorable.Add(p);
            }

            var diff = poss.Count - ignorable.Count;

            switch (diff)
            {
                case 2 :
                    var both = poss.Difference(ignorable);
                    yield return (new CellPossibility(bStart.Row, bStart.Col, both.First(bStart.Possibility)),
                        _manipulation);
                    break;
                case 1 :
                    foreach (var possibility in ignorable)
                    {
                        yield return (new CellPossibility(bStart.Row, bStart.Col, possibility), _manipulation);
                    }
                    break;
            }
        }
        
        //TODO => Rest
    }
}

public class EmptyChainManipulation : INRCZTConditionChainManipulation
{
    public void BeforeSearch(BlockChain chain)
    {
        
    }

    public void AfterSearch(BlockChain chain)
    {
       
    }
}