using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Core.Settings;
using Model.Core.Settings.Types;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility.Graphs.ConstructRules;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class AlmostHiddenSetsStrategy : SudokuStrategy
{
    public const string OfficialName = "Almost Hidden Sets";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    private readonly IntSetting _maxAhsSize;
    
    public AlmostHiddenSetsStrategy(int maxAhsSize) : base(OfficialName, Difficulty.Extreme, DefaultInstanceHandling)
    {
        _maxAhsSize = new IntSetting("Max AHS Size", "The maximum size for the almost hidden sets",
            new SliderInteractionInterface(2, 5, 1), maxAhsSize);
    }

    public override IEnumerable<ISetting> EnumerateSettings()
    {
        yield return _maxAhsSize;
    }

    public override void Apply(ISudokuSolverData solverData)
    {
        solverData.PreComputer.SimpleGraph.Construct(UnitStrongLinkConstructionRule.Instance);
        var graph = solverData.PreComputer.SimpleGraph.Graph;

        foreach (var linked in solverData.PreComputer.ConstructAlmostHiddenSetGraph(_maxAhsSize.Value))
        {
            if (linked.Cells.Length > 2) continue;

            var one = linked.One;
            var two = linked.Two;
            
            if (Process1CommonCell(solverData, one, two, graph)) return;
            if (linked.Cells.Length == 2 && Process2CommonCells(solverData, one, two)) return;
        }
    }

    private bool Process2CommonCells(ISudokuSolverData solverData, IPossibilitySet one,
        IPossibilitySet two)
    {
        foreach (var cell in one.EnumerateCells())
        {
            if (two.Positions.Contains(cell)) continue;
                    
            foreach (var possibility in solverData.PossibilitiesAt(cell).EnumeratePossibilities())
            {
                if (one.Contains(possibility)) continue;

                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
            }
        }
                    
        foreach (var cell in two.EnumerateCells())
        {
            if (one.Positions.Contains(cell)) continue;
                    
            foreach (var possibility in solverData.PossibilitiesAt(cell).EnumeratePossibilities())
            {
                if (two.Contains(possibility)) continue;

                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
            }
        }

        if (solverData.ChangeBuffer.NeedCommit())
        {
            solverData.ChangeBuffer.Commit( new AlmostHiddenSetsAndStrongLinksReportBuilder(one, two,
                Array.Empty<Edge<CellPossibility>>()));
            if (StopOnFirstCommit) return true;
        }

        return false;
    }

    private bool Process1CommonCell(ISudokuSolverData solverData, IPossibilitySet one,
        IPossibilitySet two, IGraph<CellPossibility, LinkStrength> graph)
    {
        List<Edge<CellPossibility>> links = new();

        foreach (var cell in one.EnumerateCells())
        {
            foreach (var possibility in solverData.PossibilitiesAt(cell).EnumeratePossibilities())
            {
                if (one.Contains(possibility) || two.Contains(possibility)) continue;

                var current = new CellPossibility(cell, possibility);
                foreach (var friend in graph.Neighbors(current, LinkStrength.Strong))
                {
                    foreach (var friendOfFriend in graph.Neighbors(friend, LinkStrength.Strong))
                    {
                        var asCell = friendOfFriend.ToCell();

                        if (two.Positions.Contains(asCell) && asCell != cell)
                        {
                            links.Add(new Edge<CellPossibility>(current, friend));
                            links.Add(new Edge<CellPossibility>(friend, friendOfFriend));

                            solverData.ChangeBuffer.ProposeSolutionAddition(friend);
                        }
                    }
                }
            }
        }

        if (solverData.ChangeBuffer.NeedCommit())
        {
            solverData.ChangeBuffer.Commit(new AlmostHiddenSetsAndStrongLinksReportBuilder(one, two, links));
            if (StopOnFirstCommit) return true;
        }

        return false;
    }
}

public class AlmostHiddenSetsAndStrongLinksReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly IPossibilitySet _one;
    private readonly IPossibilitySet _two;
    private readonly IReadOnlyList<Edge<CellPossibility>> _links;

    public AlmostHiddenSetsAndStrongLinksReportBuilder(IPossibilitySet one, IPossibilitySet two, IReadOnlyList<Edge<CellPossibility>> links)
    {
        _one = one;
        _two = two;
        _links = links;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>($"Almost Hidden Sets : {_one} and {_two}",
            lighter =>
        {
            foreach (var cell in _one.EnumerateCells())
            {
                foreach (var possibility in _one.PossibilitiesInCell(cell).EnumeratePossibilities())
                {
                    lighter.HighlightPossibility(possibility, cell.Row, cell.Column, StepColor.Cause1);
                }
            }
            
            foreach (var cell in _two.EnumerateCells())
            {
                foreach (var possibility in _two.PossibilitiesInCell(cell).EnumeratePossibilities())
                {
                    lighter.HighlightPossibility(possibility, cell.Row, cell.Column, StepColor.Cause2);
                }
                
            }

            foreach (var link in _links)
            {
                lighter.CreateLink(link.To, link.From, LinkStrength.Strong);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}