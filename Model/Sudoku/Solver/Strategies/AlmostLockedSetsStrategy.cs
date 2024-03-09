using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.BitSets;
using Model.Sudoku.Solver.PossibilityPosition;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Sudoku.Solver.Strategies;

public class AlmostLockedSetsStrategy : SudokuStrategy
{
    public const string OfficialName = "Almost Locked Sets";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    public AlmostLockedSetsStrategy() : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior)
    {
    }

    public override void Apply(IStrategyUser strategyUser)
    {
        foreach (var linked in strategyUser.PreComputer.ConstructAlmostLockedSetGraph())
        {
            if (linked.RestrictedCommons.Count > 2) continue;

            var restrictedCommons = linked.RestrictedCommons;
            var one = linked.One;
            var two = linked.Two;
            
            foreach (var restrictedCommon in restrictedCommons.EnumeratePossibilities())
            {
                foreach (var possibility in one.Possibilities.EnumeratePossibilities())
                {
                    if (!two.Possibilities.Contains(possibility) || possibility == restrictedCommon) continue;

                    List<Cell> coords = new();
                    coords.AddRange(one.EachCell(possibility));
                    coords.AddRange(two.EachCell(possibility));

                    foreach (var coord in Cells.SharedSeenCells(coords))
                    {
                        strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Column);
                    }
                }
            }

            if (restrictedCommons.Count == 2)
            {
                foreach (var possibility in one.Possibilities.EnumeratePossibilities())
                {
                    if (restrictedCommons.Contains(possibility) || two.Possibilities.Contains(possibility)) continue;

                    foreach (var coord in Cells.SharedSeenCells(new List<Cell>(one.EachCell(possibility))))
                    {
                        strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Column);
                    }
                }
                    
                foreach (var possibility in two.Possibilities.EnumeratePossibilities())
                {
                    if (restrictedCommons.Contains(possibility) || one.Possibilities.Contains(possibility)) continue;

                    foreach (var coord in Cells.SharedSeenCells(new List<Cell>(two.EachCell(possibility))))
                    {
                        strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Column);
                    }
                }
            }

            if(strategyUser.ChangeBuffer.Commit( new AlmostLockedSetsReportBuilder(one,
                   two, restrictedCommons)) && OnCommitBehavior == OnCommitBehavior.Return) return;
        }
    }
}

public class AlmostLockedSetsReportBuilder : IChangeReportBuilder
{
    private readonly IPossibilitiesPositions _one;
    private readonly IPossibilitiesPositions _two;
    private readonly ReadOnlyBitSet16 _restrictedCommons;

    public AlmostLockedSetsReportBuilder(IPossibilitiesPositions one, IPossibilitiesPositions two, ReadOnlyBitSet16 restrictedCommons)
    {
        _one = one;
        _two = two;
        _restrictedCommons = restrictedCommons;
    }

    public ChangeReport Build(IReadOnlyList<SolverProgress> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport( "", lighter =>
        {
            foreach (var coord in _one.EachCell())
            {
                lighter.HighlightCell(coord.Row, coord.Column, ChangeColoration.CauseOffOne);
            }

            foreach (var coord in _two.EachCell())
            {
                lighter.HighlightCell(coord.Row, coord.Column, ChangeColoration.CauseOffTwo);
            }

            foreach (var possibility in _restrictedCommons.EnumeratePossibilities())
            {
                foreach (var coord in _one.EachCell())
                {
                    if(snapshot.PossibilitiesAt(coord).Contains(possibility))
                        lighter.HighlightPossibility(possibility, coord.Row, coord.Column, ChangeColoration.Neutral);
                }
                
                foreach (var coord in _two.EachCell())
                {
                    if(snapshot.PossibilitiesAt(coord).Contains(possibility))
                        lighter.HighlightPossibility(possibility, coord.Row, coord.Column, ChangeColoration.Neutral);
                }
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
}