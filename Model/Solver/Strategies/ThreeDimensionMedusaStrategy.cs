using System.Collections.Generic;
using Model.Solver.Position;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.CellColoring;
using Model.Solver.StrategiesUtility.CellColoring.ColoringResults;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies;

public class ThreeDimensionMedusaStrategy : AbstractStrategy
{
    public const string OfficialName = "3D Medusa";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public ThreeDimensionMedusaStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior) {}
    
    public override void Apply(IStrategyManager strategyManager)
    {
        strategyManager.GraphManager.ConstructSimple(ConstructRule.UnitStrongLink, ConstructRule.CellStrongLink);
        var graph = strategyManager.GraphManager.SimpleLinkGraph;

        foreach (var coloredVertices in ColorHelper.ColorAll<CellPossibility,
                     ColoringListCollection<CellPossibility>>(ColorHelper.Algorithm.ColorWithoutRules, graph,
                     Coloring.On, strategyManager.LogsManaged))
        {
            if(coloredVertices.Count <= 1) continue;
            
            HashSet<CellPossibility> inGraph = new HashSet<CellPossibility>(coloredVertices.On);
            inGraph.UnionWith(coloredVertices.Off);

            if (SearchColor(strategyManager, coloredVertices.On, coloredVertices.Off, inGraph) ||
                SearchColor(strategyManager, coloredVertices.Off, coloredVertices.On, inGraph))
            {
                if(strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                       new SimpleColoringReportBuilder(coloredVertices, true)) && 
                        OnCommitBehavior == OnCommitBehavior.Return) return;
                
                continue;
            }
            
            SearchMix(strategyManager, coloredVertices.On, coloredVertices.Off, inGraph);
            if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                new SimpleColoringReportBuilder(coloredVertices)) && OnCommitBehavior == OnCommitBehavior.Return) return;
        }
    }

    private bool SearchColor(IStrategyManager strategyManager, IReadOnlyList<CellPossibility> toSearch,
        IReadOnlyList<CellPossibility> other, HashSet<CellPossibility> inGraph)
    {
        GridPositions[] seen = { new(), new(), new(), new(), new(), new(), new(), new(), new() };
        
        for (int i = 0; i < toSearch.Count; i++)
        {
            var first = toSearch[i];
            for (int j = i + 1; j < toSearch.Count; j++)
            {
                var second = toSearch[j];

                bool sameCell = first.Row == second.Row && first.Column == second.Column;
                bool sameUnitAndPossibility = first.Possibility == second.Possibility && first.ShareAUnit(second);

                if (sameCell || sameUnitAndPossibility)
                {
                    foreach (var coord in other)
                    {
                        strategyManager.ChangeBuffer.ProposeSolutionAddition(coord);
                    }

                    return true;
                }
            }

            var current = seen[first.Possibility - 1];
            current.FillRow(first.Row);
            current.FillColumn(first.Column);
            current.FillMiniGrid(first.Row / 3, first.Column / 3);
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
                        strategyManager.ChangeBuffer.ProposeSolutionAddition(coord);
                    }

                    return true;
                }
            }
        }

        return false;
    }

    private void SearchMix(IStrategyManager strategyManager, IReadOnlyList<CellPossibility> one,
        IReadOnlyList<CellPossibility> two, HashSet<CellPossibility> inGraph)
    {
        foreach (var first in one)
        {
            foreach (var second in two)
            {
                if (first.Possibility == second.Possibility)
                {
                    foreach (var coord in first.SharedSeenCells(second))
                    {
                        var current = new CellPossibility(coord.Row, coord.Column, first.Possibility);
                        if(inGraph.Contains(current)) continue; 
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(current);
                    }
                }
                else
                {
                    if (first.Row == second.Row && first.Column == second.Column)
                        RemoveAllExcept(strategyManager, first.Row, first.Column, first.Possibility, second.Possibility);
                    else if(first.ShareAUnit(second))
                    {
                        if(strategyManager.PossibilitiesAt(first.Row, first.Column).Peek(second.Possibility) &&
                           !inGraph.Contains(new CellPossibility(first.Row, first.Column, second.Possibility)))
                            strategyManager.ChangeBuffer.ProposePossibilityRemoval(second.Possibility, first.Row, first.Column);
                        
                        if(strategyManager.PossibilitiesAt(second.Row, second.Column).Peek(first.Possibility) &&
                           !inGraph.Contains(new CellPossibility(second.Row, second.Column, first.Possibility)))
                            strategyManager.ChangeBuffer.ProposePossibilityRemoval(first.Possibility, second.Row, second.Column);
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
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(i, row, col);
            }
        }
    }
}