using System.Collections.Generic;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Sudoku.Solver.StrategiesUtility.NRCZTChains;

namespace Model.Sudoku.Solver.Strategies.NRCZTChains;

public class ZCondition : INRCZTCondition
{
    public string Name => "Z";

    public IEnumerable<(CellPossibility, INRCZTConditionChainManipulation)> SearchEndUnderCondition(
        IStrategyManager strategyManager, ILinkGraph<CellPossibility> graph, BlockChain chain,
        CellPossibility bStart)
    {
        var all = chain.AllCellPossibilities();
        
        var possibilities = strategyManager.PossibilitiesAt(bStart.Row, bStart.Column);
        if (possibilities.Count == 3)
        {
            bool ok = true;
            
            foreach (var p in possibilities)
            {
                if (all.Contains(new CellPossibility(bStart.Row, bStart.Column, p)))
                {
                    ok = false;
                    break;
                }
            }

            if (ok)
            {
                var copy = possibilities.Copy();
                copy.Remove(bStart.Possibility);
                
                foreach (var possibility in possibilities)
                {
                    if (possibility == bStart.Possibility) continue;

                    var other = copy.First(possibility);
                    yield return (new CellPossibility(bStart.Row, bStart.Column, possibility),
                        new TargetMustSeeChainManipulation(new CellPossibility(bStart.Row, bStart.Column, other)));
                }
            }
        }

        var rowPositions = strategyManager.RowPositionsAt(bStart.Row, bStart.Possibility);
        if (rowPositions.Count > 2)
        {
            bool ok = true;
            
            foreach (var col in rowPositions)
            {
                if (all.Contains(new CellPossibility(bStart.Row, col, bStart.Possibility)))
                {
                    ok = false;
                    break;
                }
            }

            if (ok)
            {
                var copy = rowPositions.Copy();
                copy.Remove(bStart.Column);

                foreach (var col in copy)
                {
                    var other = copy.Copy();
                    other.Remove(col);

                    List<CellPossibility> others = new List<CellPossibility>();
                    foreach (var c in other)
                    {
                        others.Add(new CellPossibility(bStart.Row, c, bStart.Possibility));
                    }

                    yield return (new CellPossibility(bStart.Row, col, bStart.Possibility),
                        new TargetMustSeeChainManipulation(others));
                }
            }
        }
        
        var colPositions = strategyManager.ColumnPositionsAt(bStart.Column, bStart.Possibility);
        if (colPositions.Count > 2)
        {
            bool ok = true;
            
            foreach (var row in colPositions)
            {
                if (all.Contains(new CellPossibility(row, bStart.Column, bStart.Possibility)))
                {
                    ok = false;
                    break;
                }
            }

            if (ok)
            {
                var copy = colPositions.Copy();
                copy.Remove(bStart.Row);

                foreach (var row in copy)
                {
                    var other = copy.Copy();
                    other.Remove(row);

                    List<CellPossibility> others = new List<CellPossibility>();
                    foreach (var r in other)
                    {
                        others.Add(new CellPossibility(r, bStart.Column, bStart.Possibility));
                    }

                    yield return (new CellPossibility(row, bStart.Column, bStart.Possibility),
                        new TargetMustSeeChainManipulation(others));
                }
            }
        }
        
        var boxPositions = strategyManager.MiniGridPositionsAt(bStart.Row / 3, bStart.Column / 3,
            bStart.Possibility);
        if (boxPositions.Count > 2)
        {
            bool ok = true;
            
            foreach (var cell in boxPositions)
            {
                if (all.Contains(new CellPossibility(cell, bStart.Possibility)))
                {
                    ok = false;
                    break;
                }
            }

            if (ok)
            {
                var copy = boxPositions.Copy();
                copy.Remove(bStart.Row % 3, bStart.Column % 3);

                foreach (var pos in copy)
                {
                    var other = copy.Copy();
                    other.Remove(pos.Row % 3, pos.Column % 3);

                    List<CellPossibility> others = new List<CellPossibility>();
                    foreach (var p in other)
                    {
                        others.Add(new CellPossibility(p, bStart.Possibility));
                    }

                    yield return (new CellPossibility(pos, bStart.Possibility),
                        new TargetMustSeeChainManipulation(others));
                }
            }
        }
    }
}

public class TargetMustSeeChainManipulation : INRCZTConditionChainManipulation
{
    private readonly List<CellPossibility> _mustSee;
    private readonly List<CellPossibility> _removed;

    public TargetMustSeeChainManipulation(List<CellPossibility> mustSee)
    {
        _mustSee = mustSee;
        _removed = new List<CellPossibility>();
    }
    
    public TargetMustSeeChainManipulation(CellPossibility mustSee)
    {
        _mustSee = new List<CellPossibility>(1) { mustSee };
        _removed = new List<CellPossibility>(1);
    }

    public void BeforeSearch(BlockChain chain, ILinkGraph<CellPossibility> graph)
    {
        foreach (var cp in _mustSee)
        {
            foreach (var t in chain.PossibleTargets)
            {
                if (!graph.AreNeighbors(t, cp) || cp == t)
                {
                    _removed.Add(t);
                }
            } 
        }

        chain.PossibleTargets.ExceptWith(_removed);
    }

    public void AfterSearch(BlockChain chain, ILinkGraph<CellPossibility> graph)
    {
        chain.PossibleTargets.UnionWith(_removed);
    }
}