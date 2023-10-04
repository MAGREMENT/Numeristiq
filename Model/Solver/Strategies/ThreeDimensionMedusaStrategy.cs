using System.Collections.Generic;
using Model.Solver.Positions;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies;

public class ThreeDimensionMedusaStrategy : AbstractStrategy
{
    public const string OfficialName = "3D Medusa";
    
    public ThreeDimensionMedusaStrategy() : base(OfficialName, StrategyDifficulty.Hard) {}
    
    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        var manager = new LinkGraphManager(strategyManager);
        manager.Construct(ConstructRule.UnitStrongLink, ConstructRule.CellStrongLink);
        var graph = manager.LinkGraph;

        foreach (var coloredVertices in ColorHelper.Color<CellPossibility>(graph))
        {
            if(coloredVertices.Count <= 1) continue;
            
            HashSet<CellPossibility> inGraph = new HashSet<CellPossibility>(coloredVertices.On);
            inGraph.UnionWith(coloredVertices.Off);

            if (SearchColor(strategyManager, coloredVertices.On, coloredVertices.Off, inGraph) ||
                SearchColor(strategyManager, coloredVertices.Off, coloredVertices.On, inGraph))
            {
                strategyManager.ChangeBuffer.Push(this, new SimpleColoringReportBuilder(coloredVertices, graph, true));
                continue;
            }
            
            SearchMix(strategyManager, coloredVertices.On, coloredVertices.Off, inGraph);
            if (strategyManager.ChangeBuffer.NotEmpty())
                strategyManager.ChangeBuffer.Push(this, new SimpleColoringReportBuilder(coloredVertices, graph));
        }
    }

    private bool SearchColor(IStrategyManager strategyManager, List<CellPossibility> toSearch,
        List<CellPossibility> other, HashSet<CellPossibility> inGraph)
    {
        GridPositions[] seen = { new(), new(), new(), new(), new(), new(), new(), new(), new() };
        
        for (int i = 0; i < toSearch.Count; i++)
        {
            var first = toSearch[i];
            for (int j = i + 1; j < toSearch.Count; j++)
            {
                var second = toSearch[j];

                bool sameCell = first.Row == second.Row && first.Col == second.Col;
                bool sameUnitAndPossibility = first.Possibility == second.Possibility && first.ShareAUnit(second);

                if (sameCell || sameUnitAndPossibility)
                {
                    foreach (var coord in other)
                    {
                        strategyManager.ChangeBuffer.AddSolutionToAdd(coord);
                    }

                    return true;
                }
            }

            var current = seen[first.Possibility - 1];
            current.FillRow(first.Row);
            current.FillColumn(first.Col);
            current.FillMiniGrid(first.Row / 3, first.Col / 3);
        }

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var possibilities = strategyManager.PossibilitiesAt(row, col);
                if (possibilities.Count == 0) continue;

                bool emptied = true;
                foreach (var possibility in possibilities)
                {
                    if (!seen[possibility - 1].Peek(row, col)
                        || inGraph.Contains(new CellPossibility(row, col, possibility)))
                    {
                        emptied = false;
                        break;
                    }
                }

                if (emptied)
                {
                    foreach (var coord in other)
                    {
                        strategyManager.ChangeBuffer.AddSolutionToAdd(coord);
                    }

                    return true;
                }
            }
        }

        return false;
    }

    private void SearchMix(IStrategyManager strategyManager, List<CellPossibility> one,
        List<CellPossibility> two, HashSet<CellPossibility> inGraph)
    {
        foreach (var first in one)
        {
            foreach (var second in two)
            {
                if (first.Possibility == second.Possibility)
                {
                    foreach (var coord in first.SharedSeenCells(second))
                    {
                        var current = new CellPossibility(coord.Row, coord.Col, first.Possibility);
                        if(inGraph.Contains(current)) continue; 
                        strategyManager.ChangeBuffer.AddPossibilityToRemove(current);
                    }
                }
                else
                {
                    if (first.Row == second.Row && first.Col == second.Col)
                        RemoveAllExcept(strategyManager, first.Row, first.Col, first.Possibility, second.Possibility);
                    else if(first.ShareAUnit(second))
                    {
                        if(strategyManager.PossibilitiesAt(first.Row, first.Col).Peek(second.Possibility) &&
                           !inGraph.Contains(new CellPossibility(first.Row, first.Col, second.Possibility)))
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(second.Possibility, first.Row, first.Col);
                        
                        if(strategyManager.PossibilitiesAt(second.Row, second.Col).Peek(first.Possibility) &&
                           !inGraph.Contains(new CellPossibility(second.Row, second.Col, first.Possibility)))
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(first.Possibility, second.Row, second.Col);
                    }
                }
            }
        }
    }
    
    private void RemoveAllExcept(IStrategyManager strategyManager, int row, int col, int exceptOne, int exceptTwo)
    {
        for (int i = 1; i <= 9; i++)
        {
            if (i != exceptOne && i != exceptTwo)
            {
                strategyManager.ChangeBuffer.AddPossibilityToRemove(i, row, col);
            }
        }
    }

}