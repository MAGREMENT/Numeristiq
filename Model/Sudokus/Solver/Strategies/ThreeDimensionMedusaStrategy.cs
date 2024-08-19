using System.Collections.Generic;
using Model.Core;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Coloring;
using Model.Sudokus.Solver.Utility.Coloring.ColoringResults;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Sudokus.Solver.Utility.Graphs.ConstructRules;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class ThreeDimensionMedusaStrategy : SudokuStrategy
{
    public const string OfficialName = "3D Medusa";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public ThreeDimensionMedusaStrategy() : base(OfficialName, Difficulty.Hard, DefaultInstanceHandling) {}
    
    public override void Apply(ISudokuSolverData solverData)
    {
        solverData.PreComputer.SimpleGraph.Construct(UnitStrongLinkConstructionRule.Instance,
            CellStrongLinkConstructionRule.Instance);
        var graph = solverData.PreComputer.SimpleGraph.Graph;

        foreach (var coloredVertices in ColorHelper.ColorAll<CellPossibility,
                     ColoringListCollection<CellPossibility>>(ColorHelper.Algorithm.ColorWithoutRules, graph,
                     ElementColor.On, !solverData.FastMode))
        {
            if(coloredVertices.Count <= 1) continue;
            
            HashSet<CellPossibility> inGraph = new HashSet<CellPossibility>(coloredVertices.On);
            inGraph.UnionWith(coloredVertices.Off);

            if (SearchColor(solverData, coloredVertices.On, coloredVertices.Off, inGraph) ||
                SearchColor(solverData, coloredVertices.Off, coloredVertices.On, inGraph))
            {
                if (solverData.ChangeBuffer.NeedCommit())
                {
                    solverData.ChangeBuffer.Commit(new SimpleColoringReportBuilder(coloredVertices, true));
                    if (StopOnFirstCommit) return;
                }
                
                continue;
            }
            
            SearchMix(solverData, coloredVertices.On, coloredVertices.Off, inGraph);
            if (solverData.ChangeBuffer.NeedCommit())
            {
                solverData.ChangeBuffer.Commit(new SimpleColoringReportBuilder(coloredVertices));
                if (StopOnFirstCommit) return;
            }
        }
    }

    private bool SearchColor(ISudokuSolverData solverData, IReadOnlyList<CellPossibility> toSearch,
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
                        solverData.ChangeBuffer.ProposeSolutionAddition(coord);
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
                var possibilities = solverData.PossibilitiesAt(row, col);
                if (possibilities.Count == 0) continue;

                bool emptied = true;
                foreach (var possibility in possibilities.EnumeratePossibilities())
                {
                    if (!seen[possibility - 1].Contains(row, col)
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
                        solverData.ChangeBuffer.ProposeSolutionAddition(coord);
                    }

                    return true;
                }
            }
        }

        return false;
    }

    private void SearchMix(ISudokuSolverData solverData, IReadOnlyList<CellPossibility> one,
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
                        solverData.ChangeBuffer.ProposePossibilityRemoval(current);
                    }
                }
                else
                {
                    if (first.Row == second.Row && first.Column == second.Column)
                        RemoveAllExcept(solverData, first.Row, first.Column, first.Possibility, second.Possibility);
                    else if(first.ShareAUnit(second))
                    {
                        if(solverData.PossibilitiesAt(first.Row, first.Column).Contains(second.Possibility) &&
                           !inGraph.Contains(new CellPossibility(first.Row, first.Column, second.Possibility)))
                            solverData.ChangeBuffer.ProposePossibilityRemoval(second.Possibility, first.Row, first.Column);
                        
                        if(solverData.PossibilitiesAt(second.Row, second.Column).Contains(first.Possibility) &&
                           !inGraph.Contains(new CellPossibility(second.Row, second.Column, first.Possibility)))
                            solverData.ChangeBuffer.ProposePossibilityRemoval(first.Possibility, second.Row, second.Column);
                    }
                }
            }
        }
    }
    
    private void RemoveAllExcept(ISudokuSolverData solverData, int row, int col, int exceptOne, int exceptTwo)
    {
        for (int i = 1; i <= 9; i++)
        {
            if (i != exceptOne && i != exceptTwo)
            {
                solverData.ChangeBuffer.ProposePossibilityRemoval(i, row, col);
            }
        }
    }
}