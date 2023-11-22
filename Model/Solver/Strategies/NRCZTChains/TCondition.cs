using System.Collections.Generic;
using Model.Solver.Position;
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

        var rowPos = strategyManager.RowPositionsAt(bStart.Row, bStart.Possibility);
        if (rowPos.Count > 2)
        {
            var ignorable = new LinePositions();

            foreach (var col in rowPos)
            {
                if (col == bStart.Col) continue;

                var current = new CellPossibility(bStart.Row, col, bStart.Possibility);

                if (!all.Contains(current) && chain.IsWeaklyLinkedToAtLeastOneEnd(current)) ignorable.Add(col);
            }

            var diff = rowPos.Count - ignorable.Count;

            switch (diff)
            {
                case 2 :
                    var both = rowPos.Difference(ignorable);
                    yield return (new CellPossibility(bStart.Row, both.First(bStart.Col), bStart.Possibility),
                        _manipulation);
                    break;
                case 1 :
                    foreach (var col in ignorable)
                    {
                        yield return (new CellPossibility(bStart.Row, col, bStart.Possibility),
                            _manipulation);
                    }

                    break;
            }
        }
        
        var colPos = strategyManager.ColumnPositionsAt(bStart.Col, bStart.Possibility);
        if (colPos.Count > 2)
        {
            var ignorable = new LinePositions();

            foreach (var row in colPos)
            {
                if (row == bStart.Row) continue;

                var current = new CellPossibility(row, bStart.Col, bStart.Possibility);

                if (!all.Contains(current) && chain.IsWeaklyLinkedToAtLeastOneEnd(current)) ignorable.Add(row);
            }

            var diff = colPos.Count - ignorable.Count;

            switch (diff)
            {
                case 2 :
                    var both = colPos.Difference(ignorable);
                    yield return (new CellPossibility(both.First(bStart.Row), bStart.Col, bStart.Possibility),
                        _manipulation);
                    break;
                case 1 :
                    foreach (var row in ignorable)
                    {
                        yield return (new CellPossibility(row, bStart.Col, bStart.Possibility),
                            _manipulation);
                    }

                    break;
            }
        }

        var miniPos = strategyManager.MiniGridPositionsAt(bStart.Row / 3, bStart.Col / 3, bStart.Possibility);
        if (miniPos.Count > 2)
        {
            var ignorable = new MiniGridPositions(bStart.Row / 3, bStart.Col / 3);

            foreach (var pos in miniPos)
            {
                if (bStart.ToCell() == pos) continue;

                var current = new CellPossibility(pos, bStart.Possibility);

                if (!all.Contains(current) && chain.IsWeaklyLinkedToAtLeastOneEnd(current))
                    ignorable.Add(pos.Row % 3, pos.Col % 3);
            }

            var diff = miniPos.Count - ignorable.Count;

            switch (diff)
            {
                case 2 :
                    var both = miniPos.Difference(ignorable);
                    yield return (new CellPossibility(both.First(bStart.ToCell()), bStart.Possibility),
                        _manipulation);
                    break;
                case 1 :
                    foreach (var pos in ignorable)
                    {
                        yield return (new CellPossibility(pos, bStart.Possibility),
                            _manipulation);
                    }

                    break;
            }
        }
    }
}

public class EmptyChainManipulation : INRCZTConditionChainManipulation
{
    public void BeforeSearch(BlockChain chain, LinkGraph<CellPossibility> graph)
    {
        
    }

    public void AfterSearch(BlockChain chain, LinkGraph<CellPossibility> graph)
    {
       
    }
}