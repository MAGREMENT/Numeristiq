using System.Collections.Generic;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.CellColoring;
using Model.Sudoku.Solver.StrategiesUtility.CellColoring.ColoringResults;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Sudoku.Solver.Strategies;

public class ThreeDimensionMedusaStrategy : AbstractStrategy
{
    public const string OfficialName = "3D Medusa";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public ThreeDimensionMedusaStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior) {}
    
    public override void Apply(IStrategyUser strategyUser)
    {
        strategyUser.PreComputer.Graphs.ConstructSimple(ConstructRule.UnitStrongLink, ConstructRule.CellStrongLink);
        var graph = strategyUser.PreComputer.Graphs.SimpleLinkGraph;

        foreach (var coloredVertices in ColorHelper.ColorAll<CellPossibility,
                     ColoringListCollection<CellPossibility>>(ColorHelper.Algorithm.ColorWithoutRules, graph,
                     Coloring.On, strategyUser.LogsManaged))
        {
            if(coloredVertices.Count <= 1) continue;
            
            HashSet<CellPossibility> inGraph = new HashSet<CellPossibility>(coloredVertices.On);
            inGraph.UnionWith(coloredVertices.Off);

            if (SearchColor(strategyUser, coloredVertices.On, coloredVertices.Off, inGraph) ||
                SearchColor(strategyUser, coloredVertices.Off, coloredVertices.On, inGraph))
            {
                if(strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(this,
                       new SimpleColoringReportBuilder(coloredVertices, true)) && 
                        OnCommitBehavior == OnCommitBehavior.Return) return;
                
                continue;
            }
            
            SearchMix(strategyUser, coloredVertices.On, coloredVertices.Off, inGraph);
            if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(this,
                new SimpleColoringReportBuilder(coloredVertices)) && OnCommitBehavior == OnCommitBehavior.Return) return;
        }
    }

    private bool SearchColor(IStrategyUser strategyUser, IReadOnlyList<CellPossibility> toSearch,
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
                        strategyUser.ChangeBuffer.ProposeSolutionAddition(coord);
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
                var possibilities = strategyUser.PossibilitiesAt(row, col);
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
                        strategyUser.ChangeBuffer.ProposeSolutionAddition(coord);
                    }

                    return true;
                }
            }
        }

        return false;
    }

    private void SearchMix(IStrategyUser strategyUser, IReadOnlyList<CellPossibility> one,
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
                        strategyUser.ChangeBuffer.ProposePossibilityRemoval(current);
                    }
                }
                else
                {
                    if (first.Row == second.Row && first.Column == second.Column)
                        RemoveAllExcept(strategyUser, first.Row, first.Column, first.Possibility, second.Possibility);
                    else if(first.ShareAUnit(second))
                    {
                        if(strategyUser.PossibilitiesAt(first.Row, first.Column).Peek(second.Possibility) &&
                           !inGraph.Contains(new CellPossibility(first.Row, first.Column, second.Possibility)))
                            strategyUser.ChangeBuffer.ProposePossibilityRemoval(second.Possibility, first.Row, first.Column);
                        
                        if(strategyUser.PossibilitiesAt(second.Row, second.Column).Peek(first.Possibility) &&
                           !inGraph.Contains(new CellPossibility(second.Row, second.Column, first.Possibility)))
                            strategyUser.ChangeBuffer.ProposePossibilityRemoval(first.Possibility, second.Row, second.Column);
                    }
                }
            }
        }
    }
    
    private void RemoveAllExcept(IStrategyUser strategyUser, int row, int col, int exceptOne, int exceptTwo)
    {
        for (int i = 1; i <= 9; i++)
        {
            if (i != exceptOne && i != exceptTwo)
            {
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(i, row, col);
            }
        }
    }
}