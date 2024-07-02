using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.PossibilityPosition;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class AlmostLockedSetsStrategy : SudokuStrategy
{
    public const string OfficialName = "Almost Locked Sets";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public AlmostLockedSetsStrategy() : base(OfficialName, StepDifficulty.Extreme, DefaultInstanceHandling)
    {
    }

    public override void Apply(ISudokuSolverData solverData)
    {
        foreach (var linked in solverData.PreComputer.ConstructAlmostLockedSetGraph())
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
                    coords.AddRange(one.EnumerateCells(possibility));
                    coords.AddRange(two.EnumerateCells(possibility));

                    foreach (var coord in SudokuCellUtility.SharedSeenCells(coords))
                    {
                        solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Column);
                    }
                }
            }

            if (restrictedCommons.Count == 2)
            {
                foreach (var possibility in one.Possibilities.EnumeratePossibilities())
                {
                    if (restrictedCommons.Contains(possibility) || two.Possibilities.Contains(possibility)) continue;

                    foreach (var coord in SudokuCellUtility.SharedSeenCells(new List<Cell>(one.EnumerateCells(possibility))))
                    {
                        solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Column);
                    }
                }
                    
                foreach (var possibility in two.Possibilities.EnumeratePossibilities())
                {
                    if (restrictedCommons.Contains(possibility) || one.Possibilities.Contains(possibility)) continue;

                    foreach (var coord in SudokuCellUtility.SharedSeenCells(new List<Cell>(two.EnumerateCells(possibility))))
                    {
                        solverData.ChangeBuffer.ProposePossibilityRemoval(possibility, coord.Row, coord.Column);
                    }
                }
            }

            if(solverData.ChangeBuffer.Commit( new AlmostLockedSetsReportBuilder(one,
                   two, restrictedCommons)) && StopOnFirstPush) return;
        }
    }
}

public class AlmostLockedSetsReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
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

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            foreach (var coord in _one.EnumerateCells())
            {
                lighter.HighlightCell(coord.Row, coord.Column, ChangeColoration.CauseOffOne);
            }

            foreach (var coord in _two.EnumerateCells())
            {
                lighter.HighlightCell(coord.Row, coord.Column, ChangeColoration.CauseOffTwo);
            }

            foreach (var possibility in _restrictedCommons.EnumeratePossibilities())
            {
                foreach (var coord in _one.EnumerateCells())
                {
                    if(snapshot.PossibilitiesAt(coord).Contains(possibility))
                        lighter.HighlightPossibility(possibility, coord.Row, coord.Column, ChangeColoration.Neutral);
                }
                
                foreach (var coord in _two.EnumerateCells())
                {
                    if(snapshot.PossibilitiesAt(coord).Contains(possibility))
                        lighter.HighlightPossibility(possibility, coord.Row, coord.Column, ChangeColoration.Neutral);
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