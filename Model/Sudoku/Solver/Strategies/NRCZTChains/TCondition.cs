using System.Collections.Generic;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Sudoku.Solver.StrategiesUtility.NRCZTChains;
using Model.Utility.BitSets;

namespace Model.Sudoku.Solver.Strategies.NRCZTChains;
 
public class TCondition : INRCZTCondition
{
    private readonly INRCZTConditionChainManipulation _manipulation = new EmptyChainManipulation();

    public string Name => "T";

    public IEnumerable<(CellPossibility, INRCZTConditionChainManipulation)> SearchEndUnderCondition(
        IStrategyUser strategyUser, ILinkGraph<CellPossibility> graph, BlockChain chain, CellPossibility bStart)
    {
        var all = chain.AllCellPossibilities();
        
        var poss = strategyUser.PossibilitiesAt(bStart.Row, bStart.Column);
        if (poss.Count > 2)
        {
            var ignorable = new ReadOnlyBitSet16();
            
            foreach (var p in poss.EnumeratePossibilities())
            {
                if (p == bStart.Possibility) continue;

                var current = new CellPossibility(bStart.Row, bStart.Column, p);

                if (!all.Contains(current) && chain.IsWeaklyLinkedToAtLeastOneEnd(current))
                    ignorable += p;
            }

            var diff = poss.Count - ignorable.Count;

            switch (diff)
            {
                case 2 :
                    var both = poss - ignorable;
                    yield return (new CellPossibility(bStart.Row, bStart.Column, both.FirstPossibility(bStart.Possibility)),
                        _manipulation);
                    break;
                case 1 :
                    foreach (var possibility in ignorable.EnumeratePossibilities())
                    {
                        yield return (new CellPossibility(bStart.Row, bStart.Column, possibility), _manipulation);
                    }
                    break;
            }
        }

        var rowPos = strategyUser.RowPositionsAt(bStart.Row, bStart.Possibility);
        if (rowPos.Count > 2)
        {
            var ignorable = new LinePositions();

            foreach (var col in rowPos)
            {
                if (col == bStart.Column) continue;

                var current = new CellPossibility(bStart.Row, col, bStart.Possibility);

                if (!all.Contains(current) && chain.IsWeaklyLinkedToAtLeastOneEnd(current)) ignorable.Add(col);
            }

            var diff = rowPos.Count - ignorable.Count;

            switch (diff)
            {
                case 2 :
                    var both = rowPos.Difference(ignorable);
                    yield return (new CellPossibility(bStart.Row, both.First(bStart.Column), bStart.Possibility),
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
        
        var colPos = strategyUser.ColumnPositionsAt(bStart.Column, bStart.Possibility);
        if (colPos.Count > 2)
        {
            var ignorable = new LinePositions();

            foreach (var row in colPos)
            {
                if (row == bStart.Row) continue;

                var current = new CellPossibility(row, bStart.Column, bStart.Possibility);

                if (!all.Contains(current) && chain.IsWeaklyLinkedToAtLeastOneEnd(current)) ignorable.Add(row);
            }

            var diff = colPos.Count - ignorable.Count;

            switch (diff)
            {
                case 2 :
                    var both = colPos.Difference(ignorable);
                    yield return (new CellPossibility(both.First(bStart.Row), bStart.Column, bStart.Possibility),
                        _manipulation);
                    break;
                case 1 :
                    foreach (var row in ignorable)
                    {
                        yield return (new CellPossibility(row, bStart.Column, bStart.Possibility),
                            _manipulation);
                    }

                    break;
            }
        }

        var miniPos = strategyUser.MiniGridPositionsAt(bStart.Row / 3, bStart.Column / 3, bStart.Possibility);
        if (miniPos.Count > 2)
        {
            var ignorable = new MiniGridPositions(bStart.Row / 3, bStart.Column / 3);

            foreach (var pos in miniPos)
            {
                if (bStart.ToCell() == pos) continue;

                var current = new CellPossibility(pos, bStart.Possibility);

                if (!all.Contains(current) && chain.IsWeaklyLinkedToAtLeastOneEnd(current))
                    ignorable.Add(pos.Row % 3, pos.Column % 3);
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
    public void BeforeSearch(BlockChain chain, ILinkGraph<CellPossibility> graph)
    {
        
    }

    public void AfterSearch(BlockChain chain, ILinkGraph<CellPossibility> graph)
    {
       
    }
}