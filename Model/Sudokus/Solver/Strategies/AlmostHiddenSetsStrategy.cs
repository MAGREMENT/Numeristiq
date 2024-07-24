using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class AlmostHiddenSetsStrategy : SudokuStrategy
{
    public const string OfficialName = "Almost Hidden Sets";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public AlmostHiddenSetsStrategy() : base(OfficialName, StepDifficulty.Extreme, DefaultInstanceHandling)
    {
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        solverData.PreComputer.Graphs.ConstructSimple(SudokuConstructRuleBank.UnitStrongLink);
        var graph = solverData.PreComputer.Graphs.SimpleLinkGraph;

        foreach (var linked in solverData.PreComputer.ConstructAlmostHiddenSetGraph())
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
                if (one.Possibilities.Contains(possibility)) continue;

                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
            }
        }
                    
        foreach (var cell in two.EnumerateCells())
        {
            if (one.Positions.Contains(cell)) continue;
                    
            foreach (var possibility in solverData.PossibilitiesAt(cell).EnumeratePossibilities())
            {
                if (two.Possibilities.Contains(possibility)) continue;

                solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
            }
        }

        return solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
            new AlmostHiddenSetsAndStrongLinksReportBuilder(one, two, new List<Link<CellPossibility>>()))
                                                       && StopOnFirstPush;
    }

    private bool Process1CommonCell(ISudokuSolverData solverData, IPossibilitySet one,
        IPossibilitySet two, ILinkGraph<CellPossibility> graph)
    {
        List<Link<CellPossibility>> links = new();

        foreach (var cell in one.EnumerateCells())
        {
            foreach (var possibility in solverData.PossibilitiesAt(cell).EnumeratePossibilities())
            {
                if (one.Possibilities.Contains(possibility) || two.Possibilities.Contains(possibility)) continue;

                var current = new CellPossibility(cell, possibility);
                foreach (var friend in graph.Neighbors(current, LinkStrength.Strong))
                {
                    foreach (var friendOfFriend in graph.Neighbors(friend, LinkStrength.Strong))
                    {
                        var asCell = friendOfFriend.ToCell();

                        if (two.Positions.Contains(asCell) && asCell != cell)
                        {
                            links.Add(new Link<CellPossibility>(current, friend));
                            links.Add(new Link<CellPossibility>(friend, friendOfFriend));

                            solverData.ChangeBuffer.ProposeSolutionAddition(friend);
                        }
                    }
                }
            }
        }

        return solverData.ChangeBuffer.NotEmpty() && solverData.ChangeBuffer.Commit(
                   new AlmostHiddenSetsAndStrongLinksReportBuilder(one, two, links)) &&
                        StopOnFirstPush;
    }
}

public class AlmostHiddenSetsAndStrongLinksReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly IPossibilitySet _one;
    private readonly IPossibilitySet _two;
    private readonly List<Link<CellPossibility>> _links;

    public AlmostHiddenSetsAndStrongLinksReportBuilder(IPossibilitySet one, IPossibilitySet two, List<Link<CellPossibility>> links)
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