using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.PossibilityPosition;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Sudoku.Solver.Strategies;

public class AlmostHiddenSetsStrategy : SudokuStrategy
{
    public const string OfficialName = "Almost Hidden Sets";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    public AlmostHiddenSetsStrategy() : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
    }
    
    public override void Apply(IStrategyUser strategyUser)
    {
        strategyUser.PreComputer.Graphs.ConstructSimple(ConstructRule.UnitStrongLink);
        var graph = strategyUser.PreComputer.Graphs.SimpleLinkGraph;

        foreach (var linked in strategyUser.PreComputer.ConstructAlmostHiddenSetGraph())
        {
            if (linked.Cells.Length > 2) continue;

            var one = linked.One;
            var two = linked.Two;
            
            if (Process1CommonCell(strategyUser, one, two, graph)) return;
            if (linked.Cells.Length == 2 && Process2CommonCells(strategyUser, one, two)) return;
        }
    }

    private bool Process2CommonCells(IStrategyUser strategyUser, IPossibilitiesPositions one,
        IPossibilitiesPositions two)
    {
        foreach (var cell in one.EachCell())
        {
            if (two.Positions.Contains(cell)) continue;
                    
            foreach (var possibility in strategyUser.PossibilitiesAt(cell).EnumeratePossibilities())
            {
                if (one.Possibilities.Contains(possibility)) continue;

                strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
            }
        }
                    
        foreach (var cell in two.EachCell())
        {
            if (one.Positions.Contains(cell)) continue;
                    
            foreach (var possibility in strategyUser.PossibilitiesAt(cell).EnumeratePossibilities())
            {
                if (two.Possibilities.Contains(possibility)) continue;

                strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
            }
        }

        return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
            new AlmostHiddenSetsAndStrongLinksReportBuilder(one, two, new List<Link<CellPossibility>>()))
                                                       && OnCommitBehavior == OnCommitBehavior.Return;
    }

    private bool Process1CommonCell(IStrategyUser strategyUser, IPossibilitiesPositions one,
        IPossibilitiesPositions two, ILinkGraph<CellPossibility> graph)
    {
        List<Link<CellPossibility>> links = new();

        foreach (var cell in one.EachCell())
        {
            foreach (var possibility in strategyUser.PossibilitiesAt(cell).EnumeratePossibilities())
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

                            strategyUser.ChangeBuffer.ProposeSolutionAddition(friend);
                        }
                    }
                }
            }
        }

        return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(
                   new AlmostHiddenSetsAndStrongLinksReportBuilder(one, two, links)) &&
                        OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public class AlmostHiddenSetsAndStrongLinksReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState>
{
    private readonly IPossibilitiesPositions _one;
    private readonly IPossibilitiesPositions _two;
    private readonly List<Link<CellPossibility>> _links;

    public AlmostHiddenSetsAndStrongLinksReportBuilder(IPossibilitiesPositions one, IPossibilitiesPositions two, List<Link<CellPossibility>> links)
    {
        _one = one;
        _two = two;
        _links = links;
    }

    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport( "", lighter =>
        {
            foreach (var cell in _one.EachCell())
            {
                foreach (var possibility in _one.PossibilitiesInCell(cell).EnumeratePossibilities())
                {
                    lighter.HighlightPossibility(possibility, cell.Row, cell.Column, ChangeColoration.CauseOffOne);
                }
            }
            
            foreach (var cell in _two.EachCell())
            {
                foreach (var possibility in _two.PossibilitiesInCell(cell).EnumeratePossibilities())
                {
                    lighter.HighlightPossibility(possibility, cell.Row, cell.Column, ChangeColoration.CauseOffTwo);
                }
                
            }

            foreach (var link in _links)
            {
                lighter.CreateLink(link.To, link.From, LinkStrength.Strong);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
}