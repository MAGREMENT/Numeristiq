using System;
using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Sudokus.Solver.Utility.Graphs.ConstructRules;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.Strategies;

public class XYChainsStrategy : SudokuStrategy
{
    public const string OfficialName = "XY-Chains";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public XYChainsStrategy() : base(OfficialName, Difficulty.Hard, DefaultInstanceHandling) {}

    public override void Apply(ISudokuSolverData solverData)
    {
        solverData.PreComputer.SimpleGraph.Construct(XyChainSpecificConstructionRule.Instance,
            CellStrongLinkConstructionRule.Instance);
        var graph = solverData.PreComputer.SimpleGraph.Graph;
        var route = new List<XYCell>();
        var visited = new HashSet<CellPossibility>();

        foreach (var start in graph)
        {
            if (Search(solverData, graph, start, route, visited)) return;
            visited.Clear();
        }
    }

    private bool Search(ISudokuSolverData solverData, IGraph<CellPossibility, LinkStrength> graph, CellPossibility current,
        List<XYCell> route, HashSet<CellPossibility> visited)
    {
        var friend = graph.Neighbors(current, LinkStrength.Strong).First();
        
        route.Add(new XYCell(current.Possibility, friend.Possibility, current.Row, current.Column));
        visited.Add(current);
        
        if(friend.Possibility == route[0].XPossibility && Process(solverData, route)) return true;

        foreach (var next in graph.Neighbors(friend, LinkStrength.Weak))
        {
            if (!visited.Contains(next))
            {
                if (Search(solverData, graph, next, route, visited)) return true;
            }
        }

        route.RemoveAt(route.Count - 1);

        return false;
    }

    private bool Process(ISudokuSolverData solverData, List<XYCell> route)
    {
        foreach (var coord in route[0].SharedSeenCells(route[^1]))
        {
            solverData.ChangeBuffer.ProposePossibilityRemoval(route[0].XPossibility, coord.Row, coord.Column);
        }
        
        if (!solverData.ChangeBuffer.NeedCommit()) return false;
        solverData.ChangeBuffer.Commit(new XYChainReportBuilder(route));
        return StopOnFirstCommit;
    }
}

public readonly struct XYCell
{
    public int XPossibility { get; }
    public int YPossibility { get; }
    public int Row { get; }
    public int Column { get; }
    
    public XYCell(int xPossibility, int yPossibility, int row, int column)
    {
        XPossibility = xPossibility;
        YPossibility = yPossibility;
        Row = row;
        Column = column;
    }

    public IEnumerable<Cell> SharedSeenCells(XYCell cell)
    {
        return SudokuUtility.SharedSeenCells(Row, Column, cell.Row, cell.Column);
    }

    public CellPossibility XCellPossibility() => new(Row, Column, XPossibility);

    public CellPossibility YCellPossibility() => new(Row, Column, YPossibility);

    public override bool Equals(object? obj)
    {
        return obj is XYCell xy && xy.Row == Row && xy.Column == Column && xy.XPossibility == XPossibility
               && xy.YPossibility == YPossibility;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Column, XPossibility, YPossibility);
    }

    public override string ToString()
    {
        return $"{XPossibility}{YPossibility}r{Row + 1}c{Column + 1}";
    }
}

public class XYChainReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly XYCell[] _route;

    public XYChainReportBuilder(List<XYCell> route)
    {
        _route = route.ToArray();
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>($"XY-Chain : {_route.ToStringSequence(" - ")}", lighter =>
        {
            for (int i = 0; i < _route.Length; i++)
            {
                var xyCell = _route[i];
                
                lighter.HighlightPossibility(xyCell.XPossibility, xyCell.Row, xyCell.Column, 
                    StepColor.Cause2);
                lighter.HighlightPossibility(xyCell.YPossibility, xyCell.Row, xyCell.Column, 
                    StepColor.On);
                lighter.CreateLink(xyCell.XCellPossibility(), xyCell.YCellPossibility(), LinkStrength.Strong);

                if (i > 0)
                {
                    lighter.CreateLink(_route[i - 1].YCellPossibility(), xyCell.XCellPossibility(), LinkStrength.Weak);
                }
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}